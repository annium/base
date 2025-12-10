using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Net.Servers.Web;
using Annium.Net.WebSockets.Internal;
using Annium.Testing;
using Annium.Threading.Tasks;
using Xunit;
using NativeClientWebSocket = System.Net.WebSockets.ClientWebSocket;

namespace Annium.Net.WebSockets.Tests.Internal;

/// <summary>
/// Tests for low-level managed WebSocket functionality
/// </summary>
public class ManagedWebSocketTests : TestBase, IAsyncLifetime
{
    /// <summary>
    /// Gets the native client WebSocket instance
    /// </summary>
    private NativeClientWebSocket ClientSocket
    {
        get => field.NotNull();
        set;
    }

    /// <summary>
    /// Gets the managed WebSocket wrapper instance
    /// </summary>
    private ManagedWebSocket ManagedSocket
    {
        get => field.NotNull();
        set;
    }

    /// <summary>
    /// Log for text messages received
    /// </summary>
    private readonly TestLog<string> _texts = new();

    /// <summary>
    /// Log for binary messages received
    /// </summary>
    private readonly TestLog<string> _binaries = new();

    public ManagedWebSocketTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests sending a message when the underlying WebSocket is not connected
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
        this.Trace("assert status is closed");
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
        await using var server = RunServer(async (serverSocket, ct) => await serverSocket.ListenAsync(ct));

        this.Trace("connect and start listening");
        await ConnectAndStartListenAsync(server, TestContext.Current.CancellationToken);

        // act
        this.Trace("send message");
        var result = await SendTextAsync(message, new CancellationToken(true));

        // assert
        this.Trace("assert status is canceled");
        result.Is(WebSocketSendStatus.Canceled);

        this.Trace("done");
    }

    /// <summary>
    /// Tests sending a message after client closes the output stream
    /// </summary>
    /// <returns>Task representing the test operation</returns>
    [Fact]
    public async Task Send_ClientClosed()
    {
        this.Trace("start");

        // arrange
        const string message = "demo";

        this.Trace("run server");
        await using var server = RunServer(async (serverSocket, ct) => await serverSocket.ListenAsync(ct));

        this.Trace("connect and start listening");
        await ConnectAndStartListenAsync(server, TestContext.Current.CancellationToken);

        // act
        this.Trace("close output");
        ClientSocket
            .CloseOutputAsync(System.Net.WebSockets.WebSocketCloseStatus.Empty, string.Empty, CancellationToken.None)
            .GetAwaiter();

        this.Trace("send message");
        var result = await SendTextAsync(message, TestContext.Current.CancellationToken);

        // assert
        this.Trace("assert status is closed");
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
        await using var server = RunServerBase(
            async (_, ctx, _) =>
            {
                await ctx.WebSocket.CloseOutputAsync(
                    System.Net.WebSockets.WebSocketCloseStatus.Empty,
                    string.Empty,
                    default
                );
                Task.Delay(10, CancellationToken.None)
                    .ContinueWith(_ => serverTcs.SetResult(), CancellationToken.None)
                    .GetAwaiter();
            }
        );

        this.Trace("connect and start listening");
        await ConnectAndStartListenAsync(server, TestContext.Current.CancellationToken);

        // delay to let server close connection
        this.Trace("await server signal");
        await serverTcs.Task;

        // act
        this.Trace("send message");
        var result = await SendTextAsync(message, TestContext.Current.CancellationToken);

        // assert
        this.Trace("assert status is closed");
        result.Is(WebSocketSendStatus.Closed);

        this.Trace("done");
    }

    /// <summary>
    /// Tests sending a message after client aborts the connection
    /// </summary>
    /// <returns>Task representing the test operation</returns>
    [Fact]
    public async Task Send_ClientAborted()
    {
        this.Trace("start");

        // arrange
        const string message = "demo";

        this.Trace("run server");
        await using var server = RunServer(async (serverSocket, ct) => await serverSocket.ListenAsync(ct));

        this.Trace("connect and start listening");
        await ConnectAndStartListenAsync(server, TestContext.Current.CancellationToken);

        // act
        this.Trace("abort client socket");
        ClientSocket.Abort();

        this.Trace("send message");
        var result = await SendTextAsync(message, TestContext.Current.CancellationToken);

        // assert
        this.Trace("assert status is closed");
        result.Is(WebSocketSendStatus.Closed);

        this.Trace("done");
    }

    /// <summary>
    /// Tests sending a message after server aborts the connection
    /// </summary>
    /// <returns>Task representing the test operation</returns>
    [Fact]
    public async Task Send_ServerAborted()
    {
        this.Trace("start");

        // arrange
        const string message = "demo";
        var serverTcs = new TaskCompletionSource();

        this.Trace("run server");
        await using var server = RunServerBase(
            async (_, ctx, _) =>
            {
                this.Trace("abort server socket");
                ctx.WebSocket.Abort();

                await Task.CompletedTask;

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
            }
        );

        this.Trace("connect and start listening");
        await ConnectAndStartListenAsync(server, TestContext.Current.CancellationToken);

        // act
        // delay to let server close connection
        this.Trace("await server signal");
        await serverTcs.Task;

        this.Trace("send message");
        var result = await SendTextAsync(message, TestContext.Current.CancellationToken);

        // assert
        this.Trace("assert status is closed");
        result.Is(WebSocketSendStatus.Closed);

        this.Trace("done");
    }

    /// <summary>
    /// Tests normal message sending and echo behavior
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
        await using var server = RunServer(
            async (serverSocket, ct) =>
            {
                this.Trace("subscribe to text messages");
                serverSocket.OnTextReceived += x =>
                    serverSocket.SendTextAsync(x.ToArray(), CancellationToken.None).Await();

                this.Trace("subscribe to binary messages");
                serverSocket.OnBinaryReceived += x =>
                    serverSocket.SendBinaryAsync(x.ToArray(), CancellationToken.None).Await();

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
                await serverSocket.ListenAsync(ct);

                this.Trace("server socket closed");
            }
        );

        this.Trace("connect and start listening");
        await ConnectAndStartListenAsync(server, TestContext.Current.CancellationToken);

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
    /// Tests listening with a canceled cancellation token
    /// </summary>
    /// <returns>Task representing the test operation</returns>
    [Fact]
    public async Task Listen_Canceled()
    {
        this.Trace("start");

        // arrange
        this.Trace("run server");
        await using var server = RunServer(async (serverSocket, ct) => await serverSocket.ListenAsync(ct));

        this.Trace("connect");
        await ConnectAsync(server, TestContext.Current.CancellationToken);

        // act
        this.Trace("listen");
        var result = await ListenAsync(new CancellationToken(true));

        // assert
        this.Trace("assert status is closed local and no exception");
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
        await using var server = RunServer(async (serverSocket, ct) => await serverSocket.ListenAsync(ct));

        this.Trace("connect");
        await ConnectAsync(server, TestContext.Current.CancellationToken);

        this.Trace("close client socket");
        await ClientSocket.CloseOutputAsync(
            System.Net.WebSockets.WebSocketCloseStatus.NormalClosure,
            string.Empty,
            TestContext.Current.CancellationToken
        );

        // act
        this.Trace("listen");
        var result = await ListenAsync(TestContext.Current.CancellationToken);

        // assert
        this.Trace("assert status is closed local and no exception");
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
        await using var server = RunServerBase(
            async (_, ctx, _) =>
                await ctx.WebSocket.CloseOutputAsync(
                    System.Net.WebSockets.WebSocketCloseStatus.Empty,
                    string.Empty,
                    default
                )
        );

        this.Trace("connect");
        await ConnectAsync(server, TestContext.Current.CancellationToken);

        // act
        this.Trace("listen");
        var result = await ListenAsync(TestContext.Current.CancellationToken);

        // assert
        this.Trace("assert status is closed remote and no exception");
        result.Status.Is(WebSocketCloseStatus.ClosedRemote);
        result.Exception.IsDefault();

        this.Trace("done");
    }

    /// <summary>
    /// Tests listening behavior after client aborts connection
    /// </summary>
    /// <returns>Task representing the test operation</returns>
    [Fact]
    public async Task Listen_ClientAborted()
    {
        this.Trace("start");

        // arrange
        this.Trace("run server");
        await using var server = RunServer(async (serverSocket, ct) => await serverSocket.ListenAsync(ct));

        this.Trace("connect");
        await ConnectAsync(server, TestContext.Current.CancellationToken);

        this.Trace("listen detached");
        var listenTask = ListenAsync(TestContext.Current.CancellationToken);

        // act
        this.Trace("abort client socket");
        ClientSocket.Abort();

        this.Trace("await listen task");
        var result = await listenTask;

        // assert
        this.Trace("assert status is closed local and no exception");
        result.Status.Is(WebSocketCloseStatus.ClosedLocal);
        result.Exception.IsDefault();

        this.Trace("done");
    }

    /// <summary>
    /// Tests listening behavior after server aborts connection
    /// </summary>
    /// <returns>Task representing the test operation</returns>
    [Fact]
    public async Task Listen_ServerAborted()
    {
        this.Trace("start");

        // arrange
        var serverTcs = new TaskCompletionSource();

        this.Trace("run server");
        await using var server = RunServerBase(
            async (_, ctx, _) =>
            {
                this.Trace("abort server socket");
                ctx.WebSocket.Abort();

                await Task.CompletedTask;

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
            }
        );

        this.Trace("connect");
        await ConnectAsync(server, TestContext.Current.CancellationToken);

        this.Trace("listen detached");
        var listenTask = ListenAsync(TestContext.Current.CancellationToken);

        // act

        // delay to let server close connection
        this.Trace("await signal from server");
        await serverTcs.Task;

        this.Trace("await listen task");
        var result = await listenTask;

        // assert
        this.Trace("assert status is closed remote and no exception");
        result.Status.Is(WebSocketCloseStatus.ClosedRemote);
        result.Exception.IsDefault();

        this.Trace("done");
    }

    /// <summary>
    /// Tests normal message listening behavior
    /// </summary>
    /// <returns>Task representing the test operation</returns>
    [Fact]
    public async Task Listen_Normal()
    {
        this.Trace("start");

        // arrange
        this.Trace("generate messages");
        var messages = Enumerable.Range(0, 3).Select(x => new string((char)x, 10)).ToArray();

        this.Trace("run server");
        await using var server = RunServer(
            async (serverSocket, ct) =>
            {
                this.Trace("start sending messages");

                foreach (var message in messages)
                {
                    await serverSocket.SendTextAsync(message, ct);
                    await Task.Delay(1, CancellationToken.None);
                }

                this.Trace("done sending messages");
            }
        );

        // act
        this.Trace("connect");
        await ConnectAsync(server, TestContext.Current.CancellationToken);

        this.Trace("listen detached");
        ListenAsync(TestContext.Current.CancellationToken).GetAwaiter();

        // assert
        this.Trace("assert text messages arrive");
        await Expect.ToAsync(() => _texts.IsEqual(messages));

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
        await using var server = RunServer(
            async (serverSocket, ct) =>
            {
                this.Trace("start sending messages");

                foreach (var message in messages)
                {
                    await serverSocket.SendTextAsync(message, ct);
                    await Task.Delay(1, CancellationToken.None);
                }

                this.Trace("done sending messages");
            }
        );

        // act
        this.Trace("connect");
        await ConnectAsync(server, TestContext.Current.CancellationToken);

        this.Trace("listen detached");
        var listenTask = ListenAsync(TestContext.Current.CancellationToken);

        // assert
        this.Trace("assert text messages arrive");
        await Expect.ToAsync(() => _texts.IsEqual(messages));

        this.Trace("await listen task");
        var result = await listenTask;

        this.Trace("assert status is closed remote and no exception");
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
        await using var server = RunServer(
            async (serverSocket, ct) =>
            {
                this.Trace("start sending messages");

                foreach (var message in messages)
                {
                    await serverSocket.SendTextAsync(message, ct);
                    await Task.Delay(1, CancellationToken.None);
                }

                foreach (var message in messages)
                {
                    await serverSocket.SendBinaryAsync(message, ct);
                    await Task.Delay(1, CancellationToken.None);
                }

                this.Trace("done sending messages");
            }
        );

        // act
        this.Trace("connect");
        await ConnectAsync(server, TestContext.Current.CancellationToken);

        this.Trace("listen detached");
        var listenTask = ListenAsync(TestContext.Current.CancellationToken);

        // assert
        this.Trace("assert text messages arrive");
        await Expect.ToAsync(() => _texts.IsEqual(messages));

        this.Trace("assert binary messages arrive");
        await Expect.ToAsync(() => _binaries.IsEqual(messages));

        this.Trace("await listen task");
        var result = await listenTask;

        this.Trace("assert status is closed remote and no exception");
        result.Status.Is(WebSocketCloseStatus.ClosedRemote);
        result.Exception.IsDefault();

        this.Trace("done");
    }

    /// <summary>
    /// Initializes the test instance and sets up native and managed WebSocket instances
    /// </summary>
    /// <returns>Task representing the initialization operation</returns>
    public async ValueTask InitializeAsync()
    {
        this.Trace("start");

        ClientSocket = new NativeClientWebSocket();
        ManagedSocket = new ManagedWebSocket(ClientSocket, Logger);
        this.Trace<string, string>(
            "created pair of {clientSocket} and {managedSocket}",
            ClientSocket.GetFullId(),
            ManagedSocket.GetFullId()
        );

        ManagedSocket.OnTextReceived += x =>
        {
            var message = Encoding.UTF8.GetString(x.Span);
            _texts.Add(message);
        };
        ManagedSocket.OnBinaryReceived += x =>
        {
            var message = Encoding.UTF8.GetString(x.Span);
            _binaries.Add(message);
        };

        await Task.CompletedTask;

        this.Trace("done");
    }

    /// <summary>
    /// Disposes the test instance and cleans up WebSocket resources
    /// </summary>
    /// <returns>Task representing the disposal operation</returns>
    public async ValueTask DisposeAsync()
    {
        await Task.CompletedTask;
    }

    /// <summary>
    /// Runs a test server with the specified managed WebSocket handler
    /// </summary>
    /// <param name="handleWebSocket">Function to handle managed WebSocket connections</param>
    /// <returns>Disposable representing the running server</returns>
    private IServer RunServer(Func<ManagedWebSocket, CancellationToken, Task> handleWebSocket)
    {
        return RunServerBase(
            async (sp, ctx, ct) =>
            {
                this.Trace("start");

                var socket = new ManagedWebSocket(ctx.WebSocket, sp.Resolve<ILogger>());

                this.Trace<string>("handle {socket}", socket.GetFullId());
                await handleWebSocket(socket, ct);

                this.Trace("done");
            }
        );
    }

    /// <summary>
    /// Connects the WebSocket and starts listening for messages
    /// </summary>
    /// <param name="server">Server to connect to</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Task representing the connection and listen operation</returns>
    private async Task ConnectAndStartListenAsync(IServer server, CancellationToken ct = default)
    {
        this.Trace("start");

        await ConnectAsync(server, ct);
        ListenAsync(ct).GetAwaiter();

        this.Trace("done");
    }

    /// <summary>
    /// Connects the native WebSocket to the test server
    /// </summary>
    /// <param name="server">Server to connect to</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Task representing the connection operation</returns>
    private async Task ConnectAsync(IServer server, CancellationToken ct = default)
    {
        this.Trace("start");

        await ClientSocket.ConnectAsync(server.WebSocketsUri(), ct);

        this.Trace("done");
    }

    /// <summary>
    /// Starts listening for WebSocket messages
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Task with close result when listening completes</returns>
    private async Task<WebSocketCloseResult> ListenAsync(CancellationToken ct = default)
    {
        this.Trace("start");

        var result = await ManagedSocket.ListenAsync(ct);

        this.Trace("done");

        return result;
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

        var result = await ManagedSocket.SendTextAsync(text, ct);

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

        var result = await ManagedSocket.SendBinaryAsync(data, ct);

        this.Trace("done");

        return result;
    }
}
