using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Net.Servers.Sockets;
using Annium.Testing;
using Annium.Threading.Tasks;
using Xunit;

namespace Annium.Net.Sockets.Tests;

/// <summary>
/// Tests for client-server socket communication functionality
/// </summary>
public class ClientServerSocketTests : TestBase, IAsyncLifetime
{
    /// <summary>
    /// Gets the client socket instance
    /// </summary>
    private IClientSocket ClientSocket => _clientSocket.NotNull();

    /// <summary>
    /// The client socket instance
    /// </summary>
    private IClientSocket? _clientSocket;

    /// <summary>
    /// Collection of received messages
    /// </summary>
    private readonly List<byte[]> _messages = new();

    /// <summary>
    /// Action to handle client connection
    /// </summary>
    private Action<IClientSocket, IServer> _handleConnect = delegate
    {
        throw new NotImplementedException();
    };

    /// <summary>
    /// Function to run the server with a handler
    /// </summary>
    private Func<Func<IServerSocket, CancellationToken, Task>, IServer> _runServer = delegate
    {
        throw new NotImplementedException();
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientServerSocketTests"/> class
    /// </summary>
    /// <param name="outputHelper">The test output helper</param>
    public ClientServerSocketTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container => container.AddSocketsDefaultConnectionMonitorFactory());
    }

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
        this.Trace("send text");
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
        await using var server = _runServer(async (serverSocket, ct) => await serverSocket.WhenDisconnectedAsync(ct));

        this.Trace("connect");
        await ConnectAsync(server);

        // act
        this.Trace("send text");
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
        var serverConnectionTcs = new TaskCompletionSource();

        this.Trace("run server");
        await using var server = _runServer(
            async (serverSocket, ct) =>
            {
                this.Trace("send signal to client");
                var disconnectionTask = serverSocket.WhenDisconnectedAsync(ct);
                serverConnectionTcs.SetResult();
                await disconnectionTask;
            }
        );

        this.Trace("connect");
        await ConnectAsync(server);

        this.Trace("server connected");
        await serverConnectionTcs.Task;

        // act
        this.Trace("disconnect");
        await DisconnectAsync();

        this.Trace("send text");
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
        var clientConnectionTcs = new TaskCompletionSource();

        this.Trace("run server");
        await using var server = _runServer(
            async (serverSocket, _) =>
            {
                this.Trace("wait until client connected");
#pragma warning disable VSTHRD003
                await clientConnectionTcs.Task;
#pragma warning restore VSTHRD003

                this.Trace("disconnect server socket");
                serverSocket.Disconnect();
            }
        );

        this.Trace("connect");
        var disconnectionTask = ClientSocket.WhenDisconnectedAsync(ct: TestContext.Current.CancellationToken);
        await ConnectAsync(server);

        this.Trace("set client connection tcs");
        clientConnectionTcs.SetResult();

        this.Trace("await until disconnected");
        await disconnectionTask;

        // act
        this.Trace("send text");
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
        var serverConnectionTcs = new TaskCompletionSource();

        this.Trace("run server");
        await using var server = _runServer(
            async (serverSocket, ct) =>
            {
                this.Trace("subscribe to messages");
                serverSocket.OnReceived += x => serverSocket.SendAsync(x.ToArray(), CancellationToken.None).Await();
                this.Trace("server subscribed to messages");

                this.Trace("send signal to client");
                var disconnectionTask = serverSocket.WhenDisconnectedAsync(ct);
                serverConnectionTcs.TrySetResult();
                await disconnectionTask;

                this.Trace("server socket closed");
            }
        );

        this.Trace("connect");
        await ConnectAsync(server);

        this.Trace("server connected");
        await serverConnectionTcs.Task;

        // act && assert
        this.Trace("send");
        var binaryResult = await SendAsync(message, TestContext.Current.CancellationToken);

        this.Trace("assert ok");
        binaryResult.Is(SocketSendStatus.Ok);

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
        await using var server = _runServer(
            async (serverSocket, ct) =>
            {
                this.Trace("subscribe to messages");
                serverSocket.OnReceived += x => serverSocket.SendAsync(x.ToArray(), CancellationToken.None).Await();
                this.Trace("server subscribed to messages");

                this.Trace("send signal to client");
                var disconnectionTask = serverSocket.WhenDisconnectedAsync(ct);
                serverConnectionTcs.TrySetResult();
                await disconnectionTask;

                this.Trace("server socket closed");
            }
        );

        // act - send text
        this.Trace("connect");
        await ConnectAsync(server);

        this.Trace("server connected");
        await serverConnectionTcs.Task;

        this.Trace("send");
        var result = await SendAsync(message, TestContext.Current.CancellationToken);

        this.Trace("assert sent ok");
        result.Is(SocketSendStatus.Ok);

        this.Trace("assert message arrived");
        await Expect.ToAsync(() => _messages.Has(1));

        this.Trace("verify messages are valid");
        _messages.At(0).IsEqual(message);

        this.Trace("disconnect");
        await DisconnectAsync();

        // act - send text
        _messages.Clear();
        this.Trace("connect");
        serverConnectionTcs = new TaskCompletionSource();
        await ConnectAsync(server);

        this.Trace("server connected");
        await serverConnectionTcs.Task;

        this.Trace("send");
        result = await SendAsync(message, TestContext.Current.CancellationToken);

        this.Trace("assert sent ok");
        result.Is(SocketSendStatus.Ok);

        this.Trace("assert message arrived");
        await Expect.ToAsync(() => _messages.Has(1));

        this.Trace("verify messages are valid");
        _messages.At(0).IsEqual(message);

        this.Trace("disconnect");
        await DisconnectAsync();

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
        var clientTcs = new TaskCompletionSource();

        this.Trace("run server");
        await using var server = _runServer(
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

#pragma warning disable VSTHRD003
                await clientTcs.Task;
#pragma warning restore VSTHRD003
            }
        );

        // act
        this.Trace("connect");
        await ConnectAsync(server);

        // assert
        this.Trace("assert data arrived");
        await Expect.ToAsync(() => _messages.Has(messages.Count));

        this.Trace("verify messages are valid");
        _messages.IsEqual(messages);

        this.Trace("set client tcs");
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
        var messages = GenerateMessages(10, 100_000);
        var clientTcs = new TaskCompletionSource();

        this.Trace("run server");
        await using var server = _runServer(
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

#pragma warning disable VSTHRD003
                await clientTcs.Task;
#pragma warning restore VSTHRD003
            }
        );

        // act
        this.Trace("connect");
        var disconnectionTask = ClientSocket.WhenDisconnectedAsync(ct: TestContext.Current.CancellationToken);
        await ConnectAsync(server);

        // assert
        this.Trace("assert data arrived");
        await Expect.ToAsync(() => _messages.Has(messages.Count));

        this.Trace("verify messages are valid");
        _messages.IsEqual(messages);

        this.Trace("disconnect");
        clientTcs.SetResult();
        var result = await disconnectionTask;

        this.Trace("assert closed remote");
        result.Is(SocketCloseStatus.ClosedRemote);

        this.Trace("done");
    }

    /// <summary>
    /// Tests listening after reconnection
    /// </summary>
    /// <param name="streamType">The type of stream to test</param>
    /// <returns>A task representing the test operation</returns>
    [Theory]
    [InlineData(StreamType.Plain)]
    [InlineData(StreamType.Ssl)]
    public async Task Listen_Reconnect(StreamType streamType)
    {
        this.Trace("start");

        Configure(streamType);

        // arrange
        this.Trace("generate messages");
        var messages = GenerateMessages(10, 100_000);
        var clientTcs = new TaskCompletionSource();
        var connectionIndex = 0;
        var connectionsCount = 3;

        this.Trace("run server");
        await using var server = _runServer(
            async (serverSocket, _) =>
            {
                this.Trace("start sending messages");

                connectionIndex++;

                var complete = connectionIndex == connectionsCount;

                var i = 0;
                var breakAtMessage = complete ? int.MaxValue : new Random().Next(1, messages.Count - 1);
                foreach (var message in messages)
                {
                    i++;

                    // emulate disconnection
                    if (i == breakAtMessage)
                    {
                        this.Trace(
                            "disconnect, connection {connectionIndex}/{connectionsCount} at message#{num}",
                            connectionIndex,
                            connectionsCount,
                            i
                        );
                        return;
                    }

                    this.Trace("send message#{num}", i);
                    await serverSocket.SendAsync(message, CancellationToken.None);

                    await Task.Delay(1, CancellationToken.None);
                }

                this.Trace("sending messages complete");

                // await until 3-rd connection is handled
                this.Trace("wait for signal from client");
#pragma warning disable VSTHRD003
                await clientTcs.Task;
#pragma warning restore VSTHRD003
            }
        );

        this.Trace("set disconnect handler");
        ClientSocket.OnDisconnected += _ =>
        {
            this.Trace("disconnected, clear messages");
            _messages.Clear();
        };

        this.Trace("connect");
        await ConnectAsync(server);

        // assert
        this.Trace("assert data arrived");
        await Expect.ToAsync(() => _messages.Has(messages.Count));

        this.Trace("verify messages are valid");
        _messages.IsEqual(messages);

        this.Trace("send signal to stop server");
        clientTcs.SetResult();

        this.Trace("disconnect");
        await DisconnectAsync();

        this.Trace("done");
    }

    /// <summary>
    /// Tests listening when connection is lost
    /// </summary>
    /// <param name="streamType">The type of stream to test</param>
    /// <returns>A task representing the test operation</returns>
    [Theory]
    [InlineData(StreamType.Plain)]
    [InlineData(StreamType.Ssl)]
    public async Task Listen_ConnectionLost(StreamType streamType)
    {
        this.Trace("start");

        var serverOptions = ServerSocketOptions.Default with
        {
            Mode = SocketMode.Messaging,
            ConnectionMonitor = new ConnectionMonitorOptions
            {
                Factory = Get<IConnectionMonitorFactory>(),
                PingInterval = 60_000,
                MaxPingDelay = 300_000,
            },
        };
        Configure(streamType, serverOptions);

        // arrange
        this.Trace("generate messages");
        var disconnectCounter = 0;

        this.Trace("run server");
        await using var server = _runServer(
            async (serverSocket, ct) =>
            {
                this.Trace("start, await until disconnected");

                await serverSocket.WhenDisconnectedAsync(ct);

                this.Trace("done, disconnected");
            }
        );

        this.Trace("set disconnect handler");
        ClientSocket.OnDisconnected += _ =>
        {
            this.Trace("disconnected");
            Interlocked.Increment(ref disconnectCounter);
        };

        this.Trace("connect");
        await ConnectAsync(server);

        this.Trace("wait");
        await Task.Delay(700, TestContext.Current.CancellationToken);

        // assert
        this.Trace("assert disconnected");
        disconnectCounter.Is(1);

        this.Trace("done");
    }

    /// <summary>
    /// Initializes the test asynchronously
    /// </summary>
    /// <returns>A task representing the initialization</returns>
    public ValueTask InitializeAsync()
    {
        this.Trace("start");

        var options = ClientSocketOptions.Default with
        {
            Mode = SocketMode.Messaging,
            ReconnectDelay = 1,
            ConnectionMonitor = new ConnectionMonitorOptions
            {
                Factory = Get<IConnectionMonitorFactory>(),
                PingInterval = 100,
                MaxPingDelay = 500,
            },
        };
        _clientSocket = new ClientSocket(options, Logger);
        ClientSocket.OnReceived += x => _messages.Add(x.ToArray());

        ClientSocket.OnConnected += () => this.Trace("STATE: Connected");
        ClientSocket.OnDisconnected += status => this.Trace("STATE: Disconnected: {status}", status);
        ClientSocket.OnError += e => Assert.Fail($"Exception occured: {e}");

        this.Trace("done");

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Disposes the test resources asynchronously
    /// </summary>
    /// <returns>A task representing the disposal</returns>
    public ValueTask DisposeAsync()
    {
        this.Trace("start");

        _clientSocket?.Disconnect();

        this.Trace("done");

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Configures the client and server for the specified stream type
    /// </summary>
    /// <param name="streamType">The type of stream to configure</param>
    /// <param name="socketOptions">Optional server socket options</param>
    private void Configure(StreamType streamType, ServerSocketOptions? socketOptions = null)
    {
        this.Trace("start");
        switch (streamType)
        {
            case StreamType.Plain:
                _handleConnect = (socket, server) =>
                {
                    this.Trace("start");

                    socket.Connect(server.TcpUri());

                    this.Trace("done");
                };

                _runServer = handleSocket =>
                {
                    return RunServerBase(
                        async (sp, raw, ct) =>
                        {
                            this.Trace("start");

                            this.Trace<string>("wrap {raw} into network stream", raw.GetFullId());
                            await using var stream = new NetworkStream(raw, true);

                            this.Trace("create managed socket");
                            var options =
                                socketOptions
                                ?? ServerSocketOptions.Default with
                                {
                                    Mode = SocketMode.Messaging,
                                    ConnectionMonitor = new ConnectionMonitorOptions
                                    {
                                        Factory = Get<IConnectionMonitorFactory>(),
                                    },
                                };
                            var logger = sp.Resolve<ILogger>();
                            var socket = new ServerSocket(stream, options, logger, ct);

                            this.Trace<string>("handle {socket}", socket.GetFullId());
                            await handleSocket(socket, ct);

                            this.Trace<string>("disconnect {socket}", socket.GetFullId());
                            socket.Disconnect();

                            this.Trace("done");
                        }
                    );
                };
                break;
            case StreamType.Ssl:
                _handleConnect = (socket, server) =>
                {
                    this.Trace("start");

                    var authOptions = new SslClientAuthenticationOptions
                    {
                        RemoteCertificateValidationCallback = (_, _, _, _) => true,
                    };

                    socket.Connect(server.TcpUri(), authOptions);

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
                            await using var sslStream = new SslStream(new NetworkStream(raw, true), false);

                            this.Trace("authenticate as server");
                            await sslStream.AuthenticateAsServerAsync(
                                cert,
                                clientCertificateRequired: false,
                                checkCertificateRevocation: true
                            );

                            this.Trace("create server socket");
                            var options = ServerSocketOptions.Default with
                            {
                                Mode = SocketMode.Messaging,
                                ConnectionMonitor = new ConnectionMonitorOptions
                                {
                                    Factory = Get<IConnectionMonitorFactory>(),
                                },
                            };
                            var logger = sp.Resolve<ILogger>();
                            var socket = new ServerSocket(sslStream, options, logger, ct);

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
    /// <param name="server">Server, to connect to</param>
    /// <returns>A task representing the connection operation</returns>
    private async Task ConnectAsync(IServer server)
    {
        this.Trace("start");

        var tcs = new TaskCompletionSource();

        ClientSocket.Trace<string>("subscribe {tcs} to OnConnected", tcs.GetFullId());

        void HandleConnected()
        {
            ClientSocket.Trace<string>("set {tcs} to signaled state", tcs.GetFullId());
            tcs.SetResult();
            ClientSocket.OnConnected -= HandleConnected;
        }

        ClientSocket.OnConnected += HandleConnected;

        _handleConnect(ClientSocket, server);

        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(10));

        this.Trace("done");
    }

    /// <summary>
    /// Disconnects the client socket asynchronously
    /// </summary>
    /// <returns>A task representing the disconnection operation</returns>
    private async Task DisconnectAsync()
    {
        this.Trace("start");

        var tcs = new TaskCompletionSource();

        ClientSocket.Trace<string>("subscribe {tcs} to OnConnected", tcs.GetFullId());

        void HandleDisconnected(SocketCloseStatus status)
        {
            ClientSocket.Trace("set {tcs} to signaled state with status {status}", tcs.GetFullId(), status);
            tcs.TrySetResult();
            ClientSocket.OnDisconnected -= HandleDisconnected;
        }

        ClientSocket.OnDisconnected += HandleDisconnected;

        ClientSocket.Disconnect();

        await tcs.Task;

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
