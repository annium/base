using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Logging;
using Annium.Net.Sockets.Internal;
using Annium.Testing;
using Annium.Threading.Tasks;
using Xunit;

namespace Annium.Net.Sockets.Tests.Internal;

/// <summary>
/// Tests for client-server managed socket communication functionality
/// </summary>
public class ClientServerManagedSocketTests : TestBase, IAsyncLifetime
{
    /// <summary>
    /// Gets the client managed socket instance
    /// </summary>
    private IClientManagedSocket ClientSocket => _clientSocket.NotNull();

    /// <summary>
    /// The client managed socket instance
    /// </summary>
    private IClientManagedSocket? _clientSocket;

    /// <summary>
    /// Collection of received messages
    /// </summary>
    private readonly List<byte[]> _messages = new();

    /// <summary>
    /// Function to handle client connection asynchronously
    /// </summary>
    private Func<IClientManagedSocket, CancellationToken, Task> _handleConnectAsync = delegate
    {
        throw new NotImplementedException();
    };

    /// <summary>
    /// Function to run the server with a handler
    /// </summary>
    private Func<Func<IServerManagedSocket, CancellationToken, Task>, IAsyncDisposable> _runServer = delegate
    {
        throw new NotImplementedException();
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientServerManagedSocketTests"/> class
    /// </summary>
    /// <param name="outputHelper">The test output helper</param>
    public ClientServerManagedSocketTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests sending data when not connected
    /// </summary>
    /// <param name="streamType">The type of stream to test</param>
    /// <returns>A task representing the test operation</returns>
    [Theory]
    [InlineData(StreamType.Plain)]
    [InlineData(StreamType.Ssl)]
    public async Task Send_NotConnected(StreamType streamType)
    {
        this.Trace("start");

        Configure(streamType);

        // arrange
        var message = "demo"u8.ToArray();

        // act
        this.Trace("send message");
        var result = await SendAsync(message, TestContext.Current.CancellationToken);

        // assert
        this.Trace("assert closed");
        result.Is(SocketSendStatus.Closed);

        this.Trace("done");
    }

    /// <summary>
    /// Tests sending data when the operation is canceled
    /// </summary>
    /// <param name="streamType">The type of stream to test</param>
    /// <returns>A task representing the test operation</returns>
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
        await using var _ = _runServer(async (serverSocket, _) => await serverSocket.IsClosed);

        this.Trace("connect");
        await ConnectAsync(TestContext.Current.CancellationToken);

        // act
        this.Trace("send message");
        var result = await SendAsync(message, new CancellationToken(true));

        // assert
        this.Trace("assert canceled");
        result.Is(SocketSendStatus.Canceled);

        this.Trace("done");
    }

    /// <summary>
    /// Tests sending data when the client is closed
    /// </summary>
    /// <param name="streamType">The type of stream to test</param>
    /// <returns>A task representing the test operation</returns>
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
        await using var _ = _runServer(async (serverSocket, _) => await serverSocket.IsClosed);

        this.Trace("connect");
        await ConnectAsync(TestContext.Current.CancellationToken);

        // act
        this.Trace("disconnect client socket");
        ClientSocket.DisconnectAsync().GetAwaiter();

        this.Trace("send message");
        var result = await SendAsync(message, TestContext.Current.CancellationToken);

        // assert
        this.Trace("assert closed");
        result.Is(SocketSendStatus.Closed);

        this.Trace("done");
    }

    /// <summary>
    /// Tests sending data when the server is closed
    /// </summary>
    /// <param name="streamType">The type of stream to test</param>
    /// <returns>A task representing the test operation</returns>
    [Theory]
    [InlineData(StreamType.Plain)]
    [InlineData(StreamType.Ssl)]
    public async Task Send_ServerClosed(StreamType streamType)
    {
        this.Trace("start");

        Configure(streamType);

        // arrange
        var message = "demo"u8.ToArray();
        var serverTcs = new TaskCompletionSource();

        this.Trace("run server");
        await using var _ = _runServer(
            async (serverSocket, _) =>
            {
                this.Trace("disconnect server socket");
                await serverSocket.DisconnectAsync();

                Task.Delay(50, CancellationToken.None)
                    .ContinueWith(
                        _ =>
                        {
                            this.Trace("send signal to client");
                            serverTcs.SetResult();
                        },
                        CancellationToken.None
                    )
                    .GetAwaiter();
            }
        );

        this.Trace("connect");
        await ConnectAsync(TestContext.Current.CancellationToken);

        // delay to let server close connection
        this.Trace("await server signal");
        await serverTcs.Task;

        // act
        this.Trace("send message");
        var result = await SendAsync(message, TestContext.Current.CancellationToken);

        // assert
        this.Trace("assert closed");
        result.Is(SocketSendStatus.Closed);

        this.Trace("done");
    }

    /// <summary>
    /// Tests normal sending operation
    /// </summary>
    /// <param name="streamType">The type of stream to test</param>
    /// <returns>A task representing the test operation</returns>
    [Theory]
    [InlineData(StreamType.Plain)]
    [InlineData(StreamType.Ssl)]
    public async Task Send_Normal(StreamType streamType)
    {
        this.Trace("start");

        Configure(streamType);

        // arrange
        var message = "demo"u8.ToArray();
        var serverTcs = new TaskCompletionSource();

        this.Trace("run server");
        await using var _ = _runServer(
            async (serverSocket, _) =>
            {
                this.Trace("subscribe to messages");
                serverSocket.OnReceived += x => serverSocket.SendAsync(x.ToArray(), CancellationToken.None).Await();
                this.Trace("server subscribed to messages");

                Task.Delay(10, CancellationToken.None)
                    .ContinueWith(
                        _ =>
                        {
                            this.Trace("send signal to client");
                            serverTcs.SetResult();
                        },
                        CancellationToken.None
                    )
                    .GetAwaiter();

                this.Trace("listen server socket");
                await serverSocket.IsClosed;

                this.Trace("server socket closed");
            }
        );

        this.Trace("connect");
        await ConnectAsync(TestContext.Current.CancellationToken);

        // delay to let server close connection
        this.Trace("await server signal");
        await serverTcs.Task;

        // act
        this.Trace("send message");
        var textResult = await SendAsync(message, TestContext.Current.CancellationToken);

        // assert
        this.Trace("assert send result is ok");
        textResult.Is(SocketSendStatus.Ok);

        this.Trace("assert message is echoed back");
        await Expect.ToAsync(() => _messages.Has(1));

        this.Trace("verify messages are valid");
        _messages.At(0).IsEqual(message);

        this.Trace("done");
    }

    /// <summary>
    /// Tests sending data after reconnection
    /// </summary>
    /// <param name="streamType">The type of stream to test</param>
    /// <returns>A task representing the test operation</returns>
    [Theory]
    [InlineData(StreamType.Plain)]
    [InlineData(StreamType.Ssl)]
    public async Task Send_Reconnect(StreamType streamType)
    {
        this.Trace("start");

        Configure(streamType);

        // arrange
        var message = "demo"u8.ToArray();
        var serverConnectionTcs = new TaskCompletionSource();

        this.Trace("run server");
        await using var _ = _runServer(
            async (serverSocket, _) =>
            {
                this.Trace("subscribe to messages");
                serverSocket.OnReceived += x => serverSocket.SendAsync(x.ToArray(), CancellationToken.None).Await();
                this.Trace("server subscribed to messages");

                this.Trace("send signal to client");
                serverConnectionTcs.TrySetResult();

                this.Trace("await server socket closed");
                await serverSocket.IsClosed;

                this.Trace("server socket closed");
            }
        );

        // act - send
        this.Trace("connect");
        await ConnectAsync(TestContext.Current.CancellationToken);

        this.Trace("await server signal");
        await serverConnectionTcs.Task;

        this.Trace("send");
        var result = await SendAsync(message, TestContext.Current.CancellationToken);
        result.Is(SocketSendStatus.Ok);

        this.Trace("assert message arrived");
        await Expect.ToAsync(() => _messages.Has(1));

        this.Trace("verify messages are valid");
        _messages.At(0).IsEqual(message);

        this.Trace("disconnect");
        await ClientSocket.DisconnectAsync();

        // act - send
        _messages.Clear();
        this.Trace("connect");
        serverConnectionTcs = new TaskCompletionSource();
        await ConnectAsync(TestContext.Current.CancellationToken);

        this.Trace("await server signal");
        await serverConnectionTcs.Task;

        this.Trace("send");
        result = await SendAsync(message, TestContext.Current.CancellationToken);
        result.Is(SocketSendStatus.Ok);

        this.Trace("assert message arrived");
        await Expect.ToAsync(() => _messages.Has(1));

        this.Trace("verify messages are valid");
        _messages.At(0).IsEqual(message);

        this.Trace("disconnect");
        await ClientSocket.DisconnectAsync();

        this.Trace("done");
    }

    /// <summary>
    /// Tests listening when the operation is canceled
    /// </summary>
    /// <param name="streamType">The type of stream to test</param>
    /// <returns>A task representing the test operation</returns>
    [Theory]
    [InlineData(StreamType.Plain)]
    [InlineData(StreamType.Ssl)]
    public async Task Listen_Canceled(StreamType streamType)
    {
        this.Trace("start");

        Configure(streamType);

        // arrange
        this.Trace("run server");
        await using var _ = _runServer(async (serverSocket, _) => await serverSocket.IsClosed);

        this.Trace("connect");
        var cts = new CancellationTokenSource();
        await ConnectAsync(cts.Token);

        this.Trace("disconnect");
        await ClientSocket.DisconnectAsync();

        // act
        this.Trace("await closed state");
#pragma warning disable VSTHRD003
        var result = await ClientSocket.IsClosed;
#pragma warning restore VSTHRD003

        // assert
        this.Trace("assert closed local and no exception");
        result.Status.Is(SocketCloseStatus.ClosedLocal);
        result.Exception.IsDefault();

        this.Trace("done");
    }

    /// <summary>
    /// Tests listening when the client is closed
    /// </summary>
    /// <param name="streamType">The type of stream to test</param>
    /// <returns>A task representing the test operation</returns>
    [Theory]
    [InlineData(StreamType.Plain)]
    [InlineData(StreamType.Ssl)]
    public async Task Listen_ClientClosed(StreamType streamType)
    {
        this.Trace("start");

        Configure(streamType);

        // arrange
        this.Trace("run server");
        await using var _ = _runServer(async (serverSocket, _) => await serverSocket.IsClosed);

        this.Trace("connect");
        await ConnectAsync(TestContext.Current.CancellationToken);

        this.Trace("disconnect client socket");
        await ClientSocket.DisconnectAsync();

        // act
        this.Trace("await closed state");
#pragma warning disable VSTHRD003
        var result = await ClientSocket.IsClosed;
#pragma warning restore VSTHRD003

        // assert
        this.Trace("assert closed local and no exception");
        result.Status.Is(SocketCloseStatus.ClosedLocal);
        result.Exception.IsDefault();

        this.Trace("done");
    }

    /// <summary>
    /// Tests listening when the server is closed
    /// </summary>
    /// <param name="streamType">The type of stream to test</param>
    /// <returns>A task representing the test operation</returns>
    [Theory]
    [InlineData(StreamType.Plain)]
    [InlineData(StreamType.Ssl)]
    public async Task Listen_ServerClosed(StreamType streamType)
    {
        this.Trace("start");

        Configure(streamType);

        // arrange
        this.Trace("run server");
        await using var _ = _runServer(async (serverSocket, _) => await serverSocket.DisconnectAsync());

        this.Trace("connect");
        await ConnectAsync(TestContext.Current.CancellationToken);

        // act
        this.Trace("await closed state");
#pragma warning disable VSTHRD003
        var result = await ClientSocket.IsClosed;
#pragma warning restore VSTHRD003

        // assert
        this.Trace("assert closed remote and no exception");
        result.Status.Is(SocketCloseStatus.ClosedRemote);
        result.Exception.IsDefault();

        this.Trace("done");
    }

    /// <summary>
    /// Tests normal listening operation
    /// </summary>
    /// <param name="streamType">The type of stream to test</param>
    /// <returns>A task representing the test operation</returns>
    [Theory]
    [InlineData(StreamType.Plain)]
    [InlineData(StreamType.Ssl)]
    public async Task Listen_Normal(StreamType streamType)
    {
        this.Trace("start");

        Configure(streamType);

        // arrange
        this.Trace("generate messages");
        var messages = GenerateMessages(10, 100);

        this.Trace("run server");
        var clientTcs = new TaskCompletionSource();
        await using var _ = _runServer(
            async (serverSocket, _) =>
            {
                this.Trace("start sending messages");

                var i = 0;
                foreach (var message in messages)
                {
                    this.Trace("send message#{num}", ++i);
                    await serverSocket.SendAsync(message, CancellationToken.None);
                    await Task.Delay(1, CancellationToken.None);
                }

                this.Trace("sending messages complete");

                this.Trace("await client signal");
#pragma warning disable VSTHRD003
                await clientTcs.Task;
#pragma warning restore VSTHRD003
            }
        );

        // act
        this.Trace("connect");
        await ConnectAsync(TestContext.Current.CancellationToken);

        // assert
        this.Trace("assert data arrived");
        await Expect.ToAsync(() => _messages.Has(messages.Count), 1000);

        this.Trace("verify messages are valid");
        _messages.IsEqual(messages);

        this.Trace("send signal to server");
        clientTcs.SetResult();

        this.Trace("done");
    }

    /// <summary>
    /// Tests listening with small buffer sizes
    /// </summary>
    /// <param name="streamType">The type of stream to test</param>
    /// <returns>A task representing the test operation</returns>
    [Theory]
    [InlineData(StreamType.Plain)]
    [InlineData(StreamType.Ssl)]
    public async Task Listen_SmallBuffer(StreamType streamType)
    {
        this.Trace("start");

        Configure(streamType);

        // arrange
        this.Trace("generate messages");
        var messages = GenerateMessages(10, 100_000);

        this.Trace("run server");
        var clientTcs = new TaskCompletionSource();
        await using var _ = _runServer(
            async (serverSocket, ct) =>
            {
                this.Trace("start sending messages");

                var i = 0;
                foreach (var message in messages)
                {
                    this.Trace("send message#{num}", ++i);
                    await serverSocket.SendAsync(message, ct);
                    await Task.Delay(1, CancellationToken.None);
                }

                this.Trace("sending messages complete");

                this.Trace("await client signal");
#pragma warning disable VSTHRD003
                await clientTcs.Task;
#pragma warning restore VSTHRD003
            }
        );

        // act
        this.Trace("connect");
        await ConnectAsync(TestContext.Current.CancellationToken);

        // assert
        this.Trace("assert data arrived");
        await Expect.ToAsync(() =>
        {
            _messages.Has(messages.Count);
            _messages.IsEqual(messages);
        });

        this.Trace("send signal to server");
        clientTcs.SetResult();

        this.Trace("await closed state");
#pragma warning disable VSTHRD003
        var result = await ClientSocket.IsClosed;
#pragma warning restore VSTHRD003

        this.Trace("assert closed remote and no exception");
        result.Status.Is(SocketCloseStatus.ClosedRemote);
        result.Exception.IsDefault();

        this.Trace("done");
    }

    /// <summary>
    /// Initializes the test asynchronously
    /// </summary>
    /// <returns>A task representing the initialization</returns>
    public async ValueTask InitializeAsync()
    {
        this.Trace("start");

        var options = ManagedSocketOptions.Default with { Mode = SocketMode.Messaging };
        _clientSocket = new ClientManagedSocket(options, Logger);
        ClientSocket.OnReceived += x => _messages.Add(x.ToArray());

        await Task.CompletedTask;

        this.Trace("done");
    }

    /// <summary>
    /// Disposes the test resources asynchronously
    /// </summary>
    /// <returns>A task representing the disposal</returns>
    public ValueTask DisposeAsync()
    {
        this.Trace("start");

        _clientSocket?.Dispose();

        this.Trace("done");

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Configures the client and server for the specified stream type
    /// </summary>
    /// <param name="streamType">The type of stream to configure</param>
    private void Configure(StreamType streamType)
    {
        this.Trace("start");
        switch (streamType)
        {
            case StreamType.Plain:
                _handleConnectAsync = async (socket, ct) =>
                {
                    this.Trace("start");

                    await socket.ConnectAsync(EndPoint, null, ct);

                    this.Trace("done");
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
                            var options = ManagedSocketOptions.Default with { Mode = SocketMode.Messaging };
                            var logger = sp.Resolve<ILogger>();
                            var socket = new ServerManagedSocket(stream, options, logger, ct);

                            this.Trace<string>("handle {socket}", socket.GetFullId());
                            await handleSocket(socket, ct);

                            this.Trace("await for a while before disconnecting");
                            await Task.Delay(10, CancellationToken.None);

                            this.Trace("done");
                        }
                    );
                };
                break;
            case StreamType.Ssl:
                _handleConnectAsync = async (socket, ct) =>
                {
                    this.Trace("start");

                    var authOptions = new SslClientAuthenticationOptions
                    {
                        RemoteCertificateValidationCallback = (_, _, _, _) => true,
                    };

                    await socket.ConnectAsync(EndPoint, authOptions, ct);

                    this.Trace("done");
                };

                _runServer = handleSocket =>
                {
                    var cert = X509CertificateLoader.LoadPkcs12FromFile("keys/ecdsa_cert.pfx", []);

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
                            var options = ManagedSocketOptions.Default with { Mode = SocketMode.Messaging };
                            var logger = sp.Resolve<ILogger>();
                            var socket = new ServerManagedSocket(sslStream, options, logger, ct);

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

    /// <summary>
    /// Connects the client socket asynchronously
    /// </summary>
    /// <param name="ct">The cancellation token</param>
    /// <returns>A task representing the connection operation</returns>
    private async Task ConnectAsync(CancellationToken ct = default)
    {
        this.Trace("start");

        await _handleConnectAsync(ClientSocket, ct);

        this.Trace("done");
    }

    /// <summary>
    /// Sends data through the client socket asynchronously
    /// </summary>
    /// <param name="data">The data to send</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>The send status</returns>
    private async Task<SocketSendStatus> SendAsync(byte[] data, CancellationToken ct = default)
    {
        this.Trace("start");

        var result = await ClientSocket.SendAsync(data, ct);

        this.Trace("done");

        return result;
    }
}
