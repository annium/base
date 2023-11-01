using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Net.Sockets.Internal;
using Annium.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Net.Sockets.Tests.Internal;

public class MessagingManagedSocketTests : TestBase, IAsyncLifetime
{
    private Socket _clientSocket = default!;
    private Stream _clientStream = default!;
    private IManagedSocket _managedSocket = default!;
    private readonly List<byte[]> _messages = new();
    private Func<Socket, Task<Stream>> _createClientStreamAsync = delegate
    {
        throw new NotImplementedException();
    };
    private Func<Func<IManagedSocket, CancellationToken, Task>, IAsyncDisposable> _runServer = delegate
    {
        throw new NotImplementedException();
    };

    public MessagingManagedSocketTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    [Theory]
    [InlineData(StreamType.Plain)]
    [InlineData(StreamType.Ssl)]
    public async Task Send_Canceled(StreamType streamType)
    {
        this.Trace("start");

        Configure(streamType);

        // arrange
        var message = "demo"u8.ToArray();

        this.Trace("run server");
        await using var _ = _runServer(async (serverSocket, ct) => await serverSocket.ListenAsync(ct));

        this.Trace("connect and start listening");
        await ConnectAndStartListenAsync();

        // act
        this.Trace("send message with canceled flag");
        var result = await _managedSocket.SendAsync(message, new CancellationToken(true));

        // assert
        this.Trace("assert canceled is received");
        result.Is(SocketSendStatus.Canceled);

        this.Trace("done");
    }

    [Theory]
    [InlineData(StreamType.Plain)]
    [InlineData(StreamType.Ssl)]
    public async Task Send_ClientClosed(StreamType streamType)
    {
        this.Trace("start");

        Configure(streamType);

        // arrange
        var message = "demo"u8.ToArray();

        this.Trace("run server");
        await using var _ = _runServer(async (serverSocket, ct) => await serverSocket.ListenAsync(ct));

        this.Trace("connect and start listening");
        await ConnectAndStartListenAsync();

        // act
        this.Trace("close client socket");
        _clientSocket.Close();

        this.Trace("send message");
        var result = await _managedSocket.SendAsync(message);

        // assert
        this.Trace("assert close is received");
        result.Is(SocketSendStatus.Closed);

        this.Trace("done");
    }

    [Theory]
    [InlineData(StreamType.Plain)]
    public async Task Send_ServerClosed(StreamType streamType)
    {
        this.Trace("start");

        Configure(streamType);

        var message = "demo"u8.ToArray();
        var serverTcs = new TaskCompletionSource();

        this.Trace("run server");
        await using var _ = RunServerBase(
            async (_, socket, _) =>
            {
                socket.Close();
                await Task.Delay(10, CancellationToken.None);
                serverTcs.SetResult();
            }
        );

        this.Trace("connect and start listening");
        await ConnectAndStartListenAsync();

        // delay to let server close connection
        this.Trace("await server signal");
        await serverTcs.Task;

        // act
        this.Trace("send message");
        var result = await _managedSocket.SendAsync(message);

        // assert
        this.Trace("assert close is received");
        result.Is(SocketSendStatus.Closed);

        this.Trace("done");
    }

    [Theory]
    [InlineData(StreamType.Plain)]
    [InlineData(StreamType.Ssl)]
    public async Task Send_ExtremeMessage(StreamType streamType)
    {
        this.Trace("start");

        Configure(
            streamType,
            ManagedSocketOptionsBase.Default with
            {
                BufferSize = 16_384,
                ExtremeMessageSize = 65_536
            }
        );

        var message = GenerateMessages(1, 262_144)[0];
        var serverTcs = new TaskCompletionSource();

        this.Trace("run server");
        await using var _ = _runServer(
            async (serverSocket, ct) =>
            {
                serverSocket.OnReceived += x =>
                {
                    serverSocket.SendAsync(x.ToArray(), CancellationToken.None).GetAwaiter();
                };

                Task.Delay(10, CancellationToken.None)
                    .ContinueWith(_ => serverTcs.SetResult(), CancellationToken.None)
                    .GetAwaiter();

                await serverSocket.ListenAsync(ct);
            }
        );

        this.Trace("connect");
        await ConnectAsync();

        this.Trace("start listening");
        var listenTask = ListenAsync();

        // delay to let server setup connection
        this.Trace("await server signal");
        await serverTcs.Task;

        // act
        this.Trace("send message");
        await _managedSocket.SendAsync(message);

        // assert
        this.Trace("await listen result");
        var listenResult = await listenTask;

        this.Trace("assert closed with error");
        listenResult.Status.Is(SocketCloseStatus.ClosedRemote);
        listenResult.Exception.IsDefault();

        this.Trace("assert data not arrived");
        _messages.IsEmpty();

        this.Trace("done");
    }

    [Theory]
    [InlineData(StreamType.Plain)]
    [InlineData(StreamType.Ssl)]
    public async Task Send_Normal(StreamType streamType)
    {
        this.Trace("start");

        Configure(streamType);

        var message = "demo"u8.ToArray();
        var serverTcs = new TaskCompletionSource();

        this.Trace("run server");
        await using var _ = _runServer(
            async (serverSocket, ct) =>
            {
                serverSocket.OnReceived += x =>
                {
                    serverSocket.SendAsync(x.ToArray(), CancellationToken.None).GetAwaiter();
                };

                Task.Delay(10, CancellationToken.None)
                    .ContinueWith(_ => serverTcs.SetResult(), CancellationToken.None)
                    .GetAwaiter();

                await serverSocket.ListenAsync(ct);
            }
        );

        this.Trace("connect and start listening");
        await ConnectAndStartListenAsync();

        // delay to let server setup connection
        this.Trace("await server signal");
        await serverTcs.Task;

        // act
        this.Trace("send message");
        var messageResult = await _managedSocket.SendAsync(message);

        // assert
        this.Trace("assert ok is received");
        messageResult.Is(SocketSendStatus.Ok);

        this.Trace("assert message is echoed back");
        await Expect.To(() =>
        {
            _messages.Has(1);
            _messages.At(0).IsEqual(message);
        });

        this.Trace("done");
    }

    [Theory]
    [InlineData(StreamType.Plain)]
    [InlineData(StreamType.Ssl)]
    public async Task Listen_Canceled(StreamType streamType)
    {
        this.Trace("start");

        Configure(streamType);

        this.Trace("run server");
        await using var _ = _runServer(async (serverSocket, ct) => await serverSocket.ListenAsync(ct));

        this.Trace("connect");
        await ConnectAsync();

        // act
        this.Trace("listen with canceled token");
        var result = await ListenAsync(new CancellationToken(true));

        // assert
        this.Trace("assert closed local with no exception");
        result.Status.Is(SocketCloseStatus.ClosedLocal);
        result.Exception.IsDefault();

        this.Trace("done");
    }

    [Theory]
    [InlineData(StreamType.Plain)]
    [InlineData(StreamType.Ssl)]
    public async Task Listen_ClientClosed(StreamType streamType)
    {
        this.Trace("start");

        Configure(streamType);

        this.Trace("run server");
        await using var _ = _runServer(async (serverSocket, ct) => await serverSocket.ListenAsync(ct));

        this.Trace("connect");
        await ConnectAsync();

        this.Trace("close client socket");
        _clientSocket.Close();

        // act
        this.Trace("listen");
        var result = await ListenAsync();

        // assert
        this.Trace("assert closed local with no exception");
        result.Status.Is(SocketCloseStatus.ClosedLocal);
        result.Exception.IsDefault();

        this.Trace("done");
    }

    [Theory]
    [InlineData(StreamType.Plain)]
    public async Task Listen_ServerClosed(StreamType streamType)
    {
        this.Trace("start");

        Configure(streamType);

        this.Trace("run server");
        await using var _ = RunServerBase(
            (_, socket, _) =>
            {
                socket.Close();

                return Task.CompletedTask;
            }
        );

        this.Trace("connect");
        await ConnectAsync();

        // act
        this.Trace("listen");
        var result = await ListenAsync();

        // assert
        this.Trace("assert closed remote with no exception");
        result.Status.Is(SocketCloseStatus.ClosedRemote);
        result.Exception.IsDefault();

        this.Trace("done");
    }

    [Theory]
    [InlineData(StreamType.Plain)]
    [InlineData(StreamType.Ssl)]
    public async Task Listen_ExtremeMessage(StreamType streamType)
    {
        this.Trace("start");

        Configure(streamType);

        this.Trace("generate messages");
        var messages = GenerateMessages(1, 500);

        this.Trace("run server");
        await using var _ = _runServer(
            async (serverSocket, ct) =>
            {
                this.Trace("start sending chunks");

                var i = 0;
                foreach (var message in messages)
                {
                    this.Trace("send message#{num}", ++i);
                    await serverSocket.SendAsync(message, ct);
                    await Task.Delay(1, CancellationToken.None);
                }

                this.Trace("sending chunks complete");
            }
        );

        // act
        this.Trace("connect");
        await ConnectAsync(
            options: ManagedSocketOptionsBase.Default with
            {
                BufferSize = 10,
                ExtremeMessageSize = 100
            }
        );

        this.Trace("await listen result");
        var listenResult = await ListenAsync();

        // assert
        this.Trace("assert closed with error");
        listenResult.Status.Is(SocketCloseStatus.Error);
        listenResult.Exception.IsNotDefault().Reports("Extreme message expected");

        this.Trace("assert data not arrived");
        _messages.IsEmpty();

        this.Trace("done");
    }

    [Theory]
    [InlineData(StreamType.Plain)]
    [InlineData(StreamType.Ssl)]
    public async Task Listen_Normal(StreamType streamType)
    {
        this.Trace("start");

        Configure(streamType);

        this.Trace("generate messages");
        var messages = GenerateMessages(10, 100);

        this.Trace("run server");
        await using var _ = _runServer(
            async (serverSocket, ct) =>
            {
                this.Trace("start sending chunks");

                var i = 0;
                foreach (var message in messages)
                {
                    this.Trace("send message#{num}", ++i);
                    await serverSocket.SendAsync(message, ct);
                    await Task.Delay(1, CancellationToken.None);
                }

                this.Trace("sending chunks complete");
            }
        );

        // act
        this.Trace("connect");
        await ConnectAsync();

        this.Trace("listen detached");
        ListenAsync().GetAwaiter();

        // assert
        this.Trace("assert data arrived");
        await Expect.To(() =>
        {
            _messages.Has(messages.Count);
            _messages.IsEqual(messages);
        });

        this.Trace("done");
    }

    [Theory]
    [InlineData(StreamType.Plain)]
    [InlineData(StreamType.Ssl)]
    public async Task Listen_SmallBuffer(StreamType streamType)
    {
        this.Trace("start");

        Configure(streamType);

        this.Trace("generate messages");
        var messages = GenerateMessages(10, 100_000);

        this.Trace("run server");
        await using var _ = _runServer(
            async (serverSocket, ct) =>
            {
                this.Trace("start sending chunks");

                var i = 0;
                foreach (var message in messages)
                {
                    this.Trace("send chunk#{num}", ++i);
                    await serverSocket.SendAsync(message, ct);
                    await Task.Delay(1, CancellationToken.None);
                }

                this.Trace("sending chunks complete");
            }
        );

        // act
        this.Trace("connect");
        await ConnectAsync();

        this.Trace("listen detached");
        var listenTask = ListenAsync();

        // assert
        this.Trace("assert data arrived");
        await Expect.To(() =>
        {
            _messages.Has(messages.Count);
            _messages.IsEqual(messages);
        });

        this.Trace("await listen completion");
        var result = await listenTask;

        this.Trace("assert closed remote with no exception");
        result.Status.Is(SocketCloseStatus.ClosedRemote);
        result.Exception.IsDefault();

        this.Trace("done");
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        this.Trace("start");

        await _clientStream.DisposeAsync();

        this.Trace("done");
    }

    private void Configure(StreamType streamType, ManagedSocketOptionsBase? serverSocketOptions = null)
    {
        this.Trace("start");
        switch (streamType)
        {
            case StreamType.Plain:
                _createClientStreamAsync = async socket =>
                {
                    await Task.CompletedTask;

                    return new NetworkStream(socket);
                };

                _runServer = handleSocket =>
                {
                    return RunServerBase(
                        async (sp, raw, ct) =>
                        {
                            this.Trace("start");

                            this.Trace<string>("wrap {raw} into network stream", raw.GetFullId());
                            await using var stream = new NetworkStream(raw);

                            this.Trace("create managed socket");
                            var socket = new MessagingManagedSocket(
                                stream,
                                serverSocketOptions ?? ManagedSocketOptionsBase.Default,
                                sp.Resolve<ILogger>()
                            );

                            this.Trace<string>("handle {socket}", socket.GetFullId());
                            await handleSocket(socket, ct);

                            this.Trace("done");
                        }
                    );
                };
                break;
            case StreamType.Ssl:
                _createClientStreamAsync = async socket =>
                {
                    var networkStream = new NetworkStream(socket);
                    var sslStream = new SslStream(networkStream, false, ValidateServerCertificate, null);

                    await sslStream.AuthenticateAsClientAsync(string.Empty);

                    return sslStream;

                    bool ValidateServerCertificate(
                        object sender,
                        X509Certificate? certificate,
                        X509Chain? chain,
                        SslPolicyErrors sslPolicyErrors
                    )
                    {
                        // by design, no ssl verification in tests (cause it will require valid SSL certificate)
                        return true;
                    }
                };

                _runServer = handleSocket =>
                {
                    var cert = X509Certificate.CreateFromCertFile("keys/ecdsa_cert.pfx");

                    return RunServerBase(
                        async (sp, raw, ct) =>
                        {
                            this.Trace("start");

                            this.Trace<string>("wrap {raw} into ssl stream", raw.GetFullId());
                            await using var sslStream = new SslStream(new NetworkStream(raw), false);

                            this.Trace("authenticate as server");
                            await sslStream.AuthenticateAsServerAsync(
                                cert,
                                clientCertificateRequired: false,
                                checkCertificateRevocation: true
                            );

                            this.Trace("create managed socket");
                            var socket = new MessagingManagedSocket(
                                sslStream,
                                serverSocketOptions ?? ManagedSocketOptionsBase.Default,
                                sp.Resolve<ILogger>()
                            );

                            this.Trace<string>("handle {socket}", socket.GetFullId());
                            await handleSocket(socket, ct);

                            this.Trace("done");
                        }
                    );
                };
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(streamType), streamType, null);
        }

        this.Trace("done");
    }

    private async Task ConnectAndStartListenAsync(CancellationToken ct = default)
    {
        this.Trace("start");

        await ConnectAsync(ct);
        ListenAsync(ct).GetAwaiter();

        this.Trace("done");
    }

    private async Task ConnectAsync(CancellationToken ct = default, ManagedSocketOptionsBase? options = null)
    {
        this.Trace("start");

        _clientSocket = new Socket(SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };

        this.Trace("connect");
        await _clientSocket.ConnectAsync(EndPoint, ct);

        this.Trace("create client stream");
        _clientStream = await _createClientStreamAsync(_clientSocket);

        this.Trace("create managed socket");
        _managedSocket = new MessagingManagedSocket(_clientStream, options ?? ManagedSocketOptionsBase.Default, Logger);
        this.Trace<string, string>(
            "created pair of {clientSocket} and {managedSocket}",
            _clientSocket.GetFullId(),
            _managedSocket.GetFullId()
        );

        _managedSocket.OnReceived += x => _messages.Add(x.ToArray());

        await Task.CompletedTask;

        this.Trace("done");
    }

    private async Task<SocketCloseResult> ListenAsync(CancellationToken ct = default)
    {
        this.Trace("start");

        var result = await _managedSocket.ListenAsync(ct);

        this.Trace("done");

        return result;
    }
}
