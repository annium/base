using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Net.WebSockets.Internal;
using Annium.Testing;
using Annium.Threading.Tasks;
using Xunit;

namespace Annium.Net.WebSockets.Tests.Internal;

/// <summary>
/// Tests for client-server managed WebSocket communication scenarios
/// </summary>
public class ClientServerManagedWebSocketTests : TestBase, IAsyncLifetime
{
    /// <summary>
    /// Gets the client managed WebSocket instance
    /// </summary>
    private IClientManagedWebSocket ClientSocket => _clientSocket.NotNull();

    /// <summary>
    /// The client managed WebSocket instance
    /// </summary>
    private IClientManagedWebSocket? _clientSocket;

    /// <summary>
    /// Log for text messages received
    /// </summary>
    private readonly TestLog<string> _texts = new();

    /// <summary>
    /// Log for binary messages received
    /// </summary>
    private readonly TestLog<string> _binaries = new();

    public ClientServerManagedWebSocketTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests sending a message when managed WebSocket is not connected
    /// </summary>
    /// <returns>Task representing the test operation</returns>
    [Fact]
    public async Task Send_NotConnected()
    {
        this.Trace("start");

        // arrange
        const string message = "demo";

        // act
        this.Trace("send message");
        var result = await SendTextAsync(message, TestContext.Current.CancellationToken);

        // assert
        this.Trace("assert closed");
        result.Is(WebSocketSendStatus.Closed);

        this.Trace("done");
    }

    /// <summary>
    /// Tests sending a message with a canceled cancellation token
    /// </summary>
    /// <returns>Task representing the test operation</returns>
    [Fact]
    public async Task Send_Canceled()
    {
        this.Trace("start");

        // arrange
        const string message = "demo";

        this.Trace("run server");
        await using var _ = RunServer(async serverSocket => await serverSocket.IsClosed);

        this.Trace("connect");
        await ConnectAsync(TestContext.Current.CancellationToken);

        // act
        this.Trace("send message");
        var result = await SendTextAsync(message, new CancellationToken(true));

        // assert
        this.Trace("assert canceled");
        result.Is(WebSocketSendStatus.Canceled);

        this.Trace("done");
    }

    /// <summary>
    /// Tests sending a message after client managed WebSocket is closed
    /// </summary>
    /// <returns>Task representing the test operation</returns>
    [Fact]
    public async Task Send_ClientClosed()
    {
        this.Trace("start");

        // arrange
        const string message = "demo";

        this.Trace("run server");
        await using var _ = RunServer(async serverSocket => await serverSocket.IsClosed);

        this.Trace("connect");
        await ConnectAsync(TestContext.Current.CancellationToken);

        // act
        this.Trace("disconnect client socket");
        await ClientSocket.DisconnectAsync();

        this.Trace("send message");
        var result = await SendTextAsync(message, TestContext.Current.CancellationToken);

        // assert
        this.Trace("assert closed");
        result.Is(WebSocketSendStatus.Closed);

        this.Trace("done");
    }

    /// <summary>
    /// Tests sending a message after server closes the connection
    /// </summary>
    /// <returns>Task representing the test operation</returns>
    [Fact]
    public async Task Send_ServerClosed()
    {
        this.Trace("start");

        // arrange
        const string message = "demo";
        var serverTcs = new TaskCompletionSource();

        this.Trace("run server");
        await using var _ = RunServer(async serverSocket =>
        {
            this.Trace("disconnect server socket");
            await serverSocket.DisconnectAsync();

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
        });

        this.Trace("connect");
        await ConnectAsync(TestContext.Current.CancellationToken);

        // delay to let server close connection
        this.Trace("await server signal");
        await serverTcs.Task;

        // act
        this.Trace("send message");
        var result = await SendTextAsync(message, TestContext.Current.CancellationToken);

        // assert
        this.Trace("assert closed");
        result.Is(WebSocketSendStatus.Closed);

        this.Trace("done");
    }

    /// <summary>
    /// Tests normal message sending and echo behavior with managed WebSocket
    /// </summary>
    /// <returns>Task representing the test operation</returns>
    [Fact]
    public async Task Send_Normal()
    {
        this.Trace("start");

        // arrange
        const string message = "demo";
        var expectedMessages = new[] { message };
        var serverTcs = new TaskCompletionSource();

        this.Trace("run server");
        await using var _ = RunServer(async serverSocket =>
        {
            this.Trace("subscribe to text messages");
            serverSocket.OnTextReceived += x => serverSocket.SendTextAsync(x.ToArray(), CancellationToken.None).Await();
            this.Trace("server subscribed to text");

            this.Trace("subscribe to binary messages");
            serverSocket.OnBinaryReceived += x =>
                serverSocket.SendBinaryAsync(x.ToArray(), CancellationToken.None).Await();
            this.Trace("server subscribed to binary");

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
        });

        this.Trace("connect");
        await ConnectAsync(TestContext.Current.CancellationToken);

        // delay to let server close connection
        this.Trace("await server signal");
        await serverTcs.Task;

        // act
        this.Trace("send text message");
        var textResult = await SendTextAsync(message, TestContext.Current.CancellationToken);

        this.Trace("send binary message");
        var binaryResult = await SendBinaryAsync(message, TestContext.Current.CancellationToken);

        // assert
        this.Trace("assert text result is ok");
        textResult.Is(WebSocketSendStatus.Ok);

        this.Trace("assert binary result is ok");
        binaryResult.Is(WebSocketSendStatus.Ok);

        this.Trace("assert text messages arrive");
        await Expect.ToAsync(() => _texts.IsEqual(expectedMessages));

        this.Trace("assert binary messages arrive");
        await Expect.ToAsync(() => _binaries.IsEqual(expectedMessages));

        this.Trace("done");
    }

    /// <summary>
    /// Tests sending messages with client reconnection using managed WebSocket
    /// </summary>
    /// <returns>Task representing the test operation</returns>
    [Fact]
    public async Task Send_Reconnect()
    {
        this.Trace("start");

        // arrange
        const string message = "demo";
        var expectedMessages = new[] { message };
        var serverConnectionTcs = new TaskCompletionSource();

        this.Trace("run server");
        await using var _ = RunServer(async serverSocket =>
        {
            this.Trace("subscribe to text messages");
            serverSocket.OnTextReceived += x => serverSocket.SendTextAsync(x.ToArray(), CancellationToken.None).Await();
            this.Trace("server subscribed to text");

            this.Trace("subscribe to binary messages");
            serverSocket.OnBinaryReceived += x =>
                serverSocket.SendBinaryAsync(x.ToArray(), CancellationToken.None).Await();
            this.Trace("server subscribed to binary");

            this.Trace("send signal to client");
            serverConnectionTcs.TrySetResult();

            this.Trace("await server socket closed");
            await serverSocket.IsClosed;

            this.Trace("server socket closed");
        });

        // act - send text
        this.Trace("connect");
        await ConnectAsync(TestContext.Current.CancellationToken);

        this.Trace("await server signal");
        await serverConnectionTcs.Task;

        this.Trace("send text");
        var textResult = await SendTextAsync(message, TestContext.Current.CancellationToken);
        textResult.Is(WebSocketSendStatus.Ok);

        this.Trace("assert text message arrive");
        await Expect.ToAsync(() => _texts.IsEqual(expectedMessages));

        this.Trace("disconnect");
        await ClientSocket.DisconnectAsync();

        // act - send binary
        this.Trace("connect");
        serverConnectionTcs = new TaskCompletionSource();
        await ConnectAsync(TestContext.Current.CancellationToken);

        this.Trace("await server signal");
        await serverConnectionTcs.Task;

        this.Trace("send binary");
        var binaryResult = await SendBinaryAsync(message, TestContext.Current.CancellationToken);
        binaryResult.Is(WebSocketSendStatus.Ok);

        this.Trace("assert binary message arrive");
        await Expect.ToAsync(() => _binaries.IsEqual(expectedMessages));

        this.Trace("disconnect");
        await ClientSocket.DisconnectAsync();

        this.Trace("done");
    }

    /// <summary>
    /// Tests listening with a canceled cancellation token
    /// </summary>
    /// <returns>Task representing the test operation</returns>
    [Fact]
    public async Task Listen_Canceled()
    {
        this.Trace("start");

        // arrange
        this.Trace("run server");
        await using var _ = RunServer(async serverSocket => await serverSocket.IsClosed);

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
        result.Status.Is(WebSocketCloseStatus.ClosedLocal);
        result.Exception.IsDefault();

        this.Trace("done");
    }

    /// <summary>
    /// Tests listening behavior after client closes connection
    /// </summary>
    /// <returns>Task representing the test operation</returns>
    [Fact]
    public async Task Listen_ClientClosed()
    {
        this.Trace("start");

        // arrange
        this.Trace("run server");
        await using var _ = RunServer(async serverSocket => await serverSocket.IsClosed);

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
        result.Status.Is(WebSocketCloseStatus.ClosedLocal);
        result.Exception.IsDefault();

        this.Trace("done");
    }

    /// <summary>
    /// Tests listening behavior after server closes connection
    /// </summary>
    /// <returns>Task representing the test operation</returns>
    [Fact]
    public async Task Listen_ServerClosed()
    {
        this.Trace("start");

        // arrange
        this.Trace("run server");
        await using var _ = RunServer(async serverSocket => await serverSocket.DisconnectAsync());

        this.Trace("connect");
        await ConnectAsync(TestContext.Current.CancellationToken);

        // act
        this.Trace("await closed state");
#pragma warning disable VSTHRD003
        var result = await ClientSocket.IsClosed;
#pragma warning restore VSTHRD003

        // assert
        this.Trace("assert closed remote and no exception");
        result.Status.Is(WebSocketCloseStatus.ClosedRemote);
        result.Exception.IsDefault();

        this.Trace("done");
    }

    /// <summary>
    /// Tests normal message listening behavior with managed WebSocket
    /// </summary>
    /// <returns>Task representing the test operation</returns>
    [Fact]
    public async Task Listen_Normal()
    {
        this.Trace("start");

        // arrange
        var messages = Enumerable.Range(0, 3).Select(x => $"msg {x}").ToArray();

        this.Trace("run server");
        await using var _ = RunServer(async serverSocket =>
        {
            this.Trace("start sending messages");

            foreach (var message in messages)
            {
                await serverSocket.SendTextAsync(message);
                await Task.Delay(1, CancellationToken.None);
            }

            this.Trace("done sending messages");
        });

        // act
        this.Trace("connect");
        await ConnectAsync(TestContext.Current.CancellationToken);

        // assert
        this.Trace("assert text messages arrive");
        await Expect.ToAsync(() =>
        {
            _texts.Has(messages.Length);
            _texts.IsEqual(messages);
        });

        this.Trace("done");
    }

    /// <summary>
    /// Tests message listening with large messages that exceed buffer size
    /// </summary>
    /// <returns>Task representing the test operation</returns>
    [Fact]
    public async Task Listen_SmallBuffer()
    {
        this.Trace("start");

        // arrange
        this.Trace("generate messages");
        var messages = Enumerable.Range(0, 3).Select(x => new string((char)x, 1_000_000)).ToArray();

        this.Trace("run server");
        await using var _ = RunServer(async serverSocket =>
        {
            this.Trace("start sending messages");

            foreach (var message in messages)
            {
                await serverSocket.SendTextAsync(message);
                await Task.Delay(1, CancellationToken.None);
            }

            this.Trace("done sending messages");
        });

        // act
        this.Trace("connect");
        await ConnectAsync(TestContext.Current.CancellationToken);

        // assert
        this.Trace("assert text messages arrive");
        await Expect.ToAsync(() => _texts.IsEqual(messages));

        this.Trace("await closed state");
#pragma warning disable VSTHRD003
        var result = await ClientSocket.IsClosed;
#pragma warning restore VSTHRD003

        this.Trace("assert closed remote and no exception");
        result.Status.Is(WebSocketCloseStatus.ClosedRemote);
        result.Exception.IsDefault();

        this.Trace("done");
    }

    /// <summary>
    /// Tests listening to both text and binary message types
    /// </summary>
    /// <returns>Task representing the test operation</returns>
    [Fact]
    public async Task Listen_BothTypes()
    {
        this.Trace("start");

        // arrange
        this.Trace("generate messages");
        var messages = Enumerable.Range(0, 3).Select(x => new string((char)x, 10)).ToArray();

        this.Trace("run server");
        await using var _ = RunServer(async serverSocket =>
        {
            this.Trace("start sending messages");

            foreach (var message in messages)
            {
                await serverSocket.SendTextAsync(message);
                await Task.Delay(1, CancellationToken.None);
            }

            foreach (var message in messages)
            {
                await serverSocket.SendBinaryAsync(message);
                await Task.Delay(1, CancellationToken.None);
            }

            this.Trace("done sending messages");
        });

        // act
        await ConnectAsync(TestContext.Current.CancellationToken);

        // assert
        this.Trace("assert text messages arrive");
        await Expect.ToAsync(() => _texts.IsEqual(messages));

        this.Trace("assert binary messages arrive");
        await Expect.ToAsync(() => _binaries.IsEqual(messages));

        this.Trace("await closed state");
#pragma warning disable VSTHRD003
        var result = await ClientSocket.IsClosed;
#pragma warning restore VSTHRD003

        this.Trace("assert closed remote and no exception");
        result.Status.Is(WebSocketCloseStatus.ClosedRemote);
        result.Exception.IsDefault();

        this.Trace("done");
    }

    /// <summary>
    /// Initializes the test instance and sets up managed WebSocket client
    /// </summary>
    /// <returns>Task representing the initialization operation</returns>
    public async ValueTask InitializeAsync()
    {
        this.Trace("start");

        _clientSocket = new ClientManagedWebSocket(1_000, Logger);
        ClientSocket.OnTextReceived += x =>
        {
            var message = Encoding.UTF8.GetString(x.Span);
            _texts.Add(message);
        };
        ClientSocket.OnBinaryReceived += x =>
        {
            var message = Encoding.UTF8.GetString(x.ToArray());
            _binaries.Add(message);
        };

        await Task.CompletedTask;

        this.Trace("done");
    }

    /// <summary>
    /// Disposes the test instance and cleans up managed WebSocket client
    /// </summary>
    /// <returns>Task representing the disposal operation</returns>
    public async ValueTask DisposeAsync()
    {
        this.Trace("start");

        if (_clientSocket is not null)
            await _clientSocket.DisconnectAsync();

        this.Trace("done");
    }

    /// <summary>
    /// Runs a test server with the specified managed WebSocket handler
    /// </summary>
    /// <param name="handleWebSocket">Function to handle managed WebSocket connections</param>
    /// <returns>Disposable representing the running server</returns>
    private IAsyncDisposable RunServer(Func<IServerManagedWebSocket, Task> handleWebSocket)
    {
        return RunServerBase(
            async (sp, ctx, ct) =>
            {
                this.Trace("start");

                var socket = new ServerManagedWebSocket(ctx.WebSocket, sp.Resolve<ILogger>(), ct);

                this.Trace<string>("handle {socket}", socket.GetFullId());
                await handleWebSocket(socket);

                this.Trace<string>("disconnect {socket}", socket.GetFullId());
                await socket.DisconnectAsync();

                this.Trace("done");
            }
        );
    }

    /// <summary>
    /// Connects the client managed WebSocket to the test server
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Task representing the connection operation</returns>
    private async Task ConnectAsync(CancellationToken ct = default)
    {
        this.Trace("start");

        await ClientSocket.ConnectAsync(ServerUri, ct);

        this.Trace("done");
    }

    /// <summary>
    /// Sends a text message through the managed WebSocket
    /// </summary>
    /// <param name="text">The text message to send</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Task with send status result</returns>
    private async Task<WebSocketSendStatus> SendTextAsync(string text, CancellationToken ct = default)
    {
        this.Trace("start");

        var result = await ClientSocket.SendTextAsync(text, ct);

        this.Trace("done");

        return result;
    }

    /// <summary>
    /// Sends a binary message through the managed WebSocket
    /// </summary>
    /// <param name="data">The string data to convert and send as binary</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Task with send status result</returns>
    private async Task<WebSocketSendStatus> SendBinaryAsync(string data, CancellationToken ct = default)
    {
        this.Trace("start");

        var result = await ClientSocket.SendBinaryAsync(data, ct);

        this.Trace("done");

        return result;
    }
}
