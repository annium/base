using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

namespace Annium.Net.Sockets.Tests.Internal;

/// <summary>
/// Tests for raw managed socket functionality
/// </summary>
public class RawManagedSocketTests : TestBase, IAsyncLifetime
{
    /// <summary>
    /// Gets the client socket instance
    /// </summary>
    private Socket ClientSocket => _clientSocket.NotNull();

    /// <summary>
    /// Gets the client stream instance
    /// </summary>
    private Stream ClientStream => _clientStream.NotNull();

    /// <summary>
    /// Gets the managed socket instance
    /// </summary>
    private IManagedSocket ManagedSocket => _managedSocket.NotNull();

    /// <summary>
    /// The client socket instance
    /// </summary>
    private Socket? _clientSocket;

    /// <summary>
    /// The client stream instance
    /// </summary>
    private Stream? _clientStream;

    /// <summary>
    /// The managed socket instance
    /// </summary>
    private IManagedSocket? _managedSocket;

    /// <summary>
    /// The stream of received bytes
    /// </summary>
    private readonly List<byte> _stream = new();

    /// <summary>
    /// Function to create client stream asynchronously
    /// </summary>
    private Func<Socket, Task<Stream>> _createClientStreamAsync = delegate
    {
        throw new NotImplementedException();
    };

    /// <summary>
    /// Function to run the server with a handler
    /// </summary>
    private Func<Func<IManagedSocket, CancellationToken, Task>, IAsyncDisposable> _runServer = delegate
    {
        throw new NotImplementedException();
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="RawManagedSocketTests"/> class
    /// </summary>
    /// <param name="outputHelper">The test output helper</param>
    public RawManagedSocketTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

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
        await using var _ = _runServer(async (serverSocket, ct) => await serverSocket.ListenAsync(ct));

        this.Trace("connect and start listening");
        await ConnectAndStartListenAsync(TestContext.Current.CancellationToken);

        // act
        this.Trace("send message with canceled flag");
        var result = await ManagedSocket.SendAsync(message, new CancellationToken(true));

        // assert
        this.Trace("assert canceled is received");
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
        await using var _ = _runServer(async (serverSocket, ct) => await serverSocket.ListenAsync(ct));

        this.Trace("connect and start listening");
        await ConnectAndStartListenAsync(TestContext.Current.CancellationToken);

        // act
        this.Trace("close client socket");
        ClientSocket.Close();

        this.Trace("send message");
        var result = await ManagedSocket.SendAsync(message, TestContext.Current.CancellationToken);

        // assert
        this.Trace("assert close is received");
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
                await Task.Delay(50, CancellationToken.None);
                socket.LingerState = new LingerOption(true, 0);
                socket.Close();
                await Task.Delay(50, CancellationToken.None);
                serverTcs.SetResult();
            }
        );

        this.Trace("connect and start listening");
        await ConnectAndStartListenAsync(TestContext.Current.CancellationToken);

        // delay to let server close connection
        this.Trace("await server signal");
        await serverTcs.Task;

        // act
        this.Trace("send message");
        var result = await ManagedSocket.SendAsync(message, TestContext.Current.CancellationToken);

        // assert
        this.Trace("assert close is received");
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
        await ConnectAndStartListenAsync(TestContext.Current.CancellationToken);

        // delay to let server close connection
        this.Trace("await server signal");
        await serverTcs.Task;

        // act
        this.Trace("send message");
        var messageResult = await ManagedSocket.SendAsync(message, TestContext.Current.CancellationToken);

        // assert
        this.Trace("assert ok is received");
        messageResult.Is(SocketSendStatus.Ok);

        this.Trace("assert message is echoed back");
        await Expect.ToAsync(() => _stream.Has(message.Length));

        this.Trace("verify stream to be equal to message");
        _stream.IsEqual(message);

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

        this.Trace("run server");
        await using var _ = _runServer(async (serverSocket, ct) => await serverSocket.ListenAsync(ct));

        this.Trace("connect");
        await ConnectAsync(TestContext.Current.CancellationToken);

        // act
        this.Trace("listen with canceled token");
        var result = await ListenAsync(new CancellationToken(true));

        // assert
        this.Trace("assert closed local with no exception");
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

        this.Trace("run server");
        await using var _ = _runServer(async (serverSocket, ct) => await serverSocket.ListenAsync(ct));

        this.Trace("connect");
        await ConnectAsync(TestContext.Current.CancellationToken);

        this.Trace("close client socket");
        ClientSocket.Close();

        // act
        this.Trace("listen");
        var result = await ListenAsync(TestContext.Current.CancellationToken);

        // assert
        this.Trace("assert closed local with no exception");
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
    public async Task Listen_ServerClosed(StreamType streamType)
    {
        this.Trace("start");

        Configure(streamType);

        this.Trace("run server");
        await using var _ = RunServerBase(
            async (_, socket, _) =>
            {
                await Task.Delay(50, CancellationToken.None);
                socket.LingerState = new LingerOption(true, 0);
                socket.Close();
            }
        );

        this.Trace("connect");
        await ConnectAsync(TestContext.Current.CancellationToken);

        // act
        this.Trace("listen");
        var result = await ListenAsync(TestContext.Current.CancellationToken);

        // assert
        this.Trace("assert closed remote with no exception");
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

        this.Trace("generate messages");
        var (message, chunks) = GenerateMessage(100, 10);

        this.Trace("run server");
        await using var _ = _runServer(
            async (serverSocket, ct) =>
            {
                this.Trace("start sending chunks");

                var i = 0;
                foreach (var chunk in chunks)
                {
                    this.Trace("send chunk#{num}", ++i);
                    await serverSocket.SendAsync(chunk, ct);
                    await Task.Delay(1, CancellationToken.None);
                }

                this.Trace("sending chunks complete");
            }
        );

        // act
        this.Trace("connect");
        await ConnectAsync(TestContext.Current.CancellationToken);

        this.Trace("listen detached");
        ListenAsync(TestContext.Current.CancellationToken).GetAwaiter();

        // assert
        this.Trace("assert data arrived");
        await Expect.ToAsync(() => _stream.Has(message.Length), 1000);

        this.Trace("verify stream to be equal to message");
        _stream.IsEqual(message);

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

        this.Trace("generate messages");
        var (message, chunks) = GenerateMessage(1_000_000, 100_000);

        this.Trace("run server");
        await using var _ = _runServer(
            async (serverSocket, ct) =>
            {
                this.Trace("start sending chunks");

                var i = 0;
                foreach (var chunk in chunks)
                {
                    this.Trace("send chunk#{num}", ++i);
                    await serverSocket.SendAsync(chunk, ct);
                    await Task.Delay(1, CancellationToken.None);
                }

                this.Trace("sending chunks complete");
            }
        );

        // act
        this.Trace("connect");
        await ConnectAsync(TestContext.Current.CancellationToken);

        this.Trace("listen detached");
        var listenTask = ListenAsync(TestContext.Current.CancellationToken);

        // assert
        this.Trace("assert data arrived");
        await Expect.ToAsync(() => _stream.Count.Is(message.Length));

        this.Trace("verify stream to be equal to message");
        _stream.SequenceEqual(message).IsTrue();

        this.Trace("await listen completion");
        var result = await listenTask;

        this.Trace("assert closed remote with no exception");
        result.Status.Is(SocketCloseStatus.ClosedRemote);
        result.Exception.IsDefault();

        this.Trace("done");
    }

    /// <summary>
    /// Initializes the test asynchronously
    /// </summary>
    /// <returns>A task representing the initialization</returns>
    public ValueTask InitializeAsync()
    {
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Disposes the test resources asynchronously
    /// </summary>
    /// <returns>A task representing the disposal</returns>
    public async ValueTask DisposeAsync()
    {
        this.Trace("start");

        if (_clientStream is not null)
            await _clientStream.DisposeAsync();

        this.Trace("done");
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
                            var socket = new RawManagedSocket(
                                stream,
                                ManagedSocketOptionsBase.Default,
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
                            var socket = new RawManagedSocket(
                                sslStream,
                                ManagedSocketOptionsBase.Default,
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

    /// <summary>
    /// Connects the client and starts listening asynchronously
    /// </summary>
    /// <param name="ct">The cancellation token</param>
    /// <returns>A task representing the operation</returns>
    private async Task ConnectAndStartListenAsync(CancellationToken ct = default)
    {
        this.Trace("start");

        await ConnectAsync(ct);
        ListenAsync(ct).GetAwaiter();

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

        _clientSocket = new Socket(SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };

        this.Trace("connect");
        await ClientSocket.ConnectAsync(EndPoint, ct);

        this.Trace("create client stream");
        _clientStream = await _createClientStreamAsync(ClientSocket);

        this.Trace("create managed socket");
        _managedSocket = new RawManagedSocket(ClientStream, ManagedSocketOptionsBase.Default, Logger);
        this.Trace<string, string>(
            "created pair of {clientSocket} and {managedSocket}",
            ClientSocket.GetFullId(),
            ManagedSocket.GetFullId()
        );

        ManagedSocket.OnReceived += x => _stream.AddRange(x.ToArray());

        await Task.CompletedTask;

        this.Trace("done");
    }

    /// <summary>
    /// Listens for incoming data asynchronously
    /// </summary>
    /// <param name="ct">The cancellation token</param>
    /// <returns>The socket close result</returns>
    private async Task<SocketCloseResult> ListenAsync(CancellationToken ct = default)
    {
        this.Trace("start");

        var result = await ManagedSocket.ListenAsync(ct);

        this.Trace("done");

        return result;
    }
}
