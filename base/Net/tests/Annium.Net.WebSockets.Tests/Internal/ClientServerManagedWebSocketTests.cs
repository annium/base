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

namespace Annium.Net.WebSockets.Tests.Internal;

/// <summary>
/// Tests for client-server managed WebSocket communication scenarios
/// </summary>
public class ClientServerManagedWebSocketTests : TestBase
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

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientServerManagedWebSocketTests"/> class.
    /// </summary>
    /// <param name="outputHelper">xUnit test output helper the test host logs through.</param>
    public ClientServerManagedWebSocketTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Verifies that <see cref="ClientManagedWebSocket.ConnectAsync"/> returns a non-null exception
    /// when the target endpoint refuses the connection (nothing listening on that port), and that
    /// the socket remains in its initial closed state (IsClosed already completed with ClosedLocal).
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task ConnectAsync_ConnectionRefused_ReturnsException()
    {
        this.Trace("start");

        // Use a URI that is guaranteed to have nothing listening (port 1 is reserved and
        // requires root on most OSes, so connection will always be refused immediately).
        var unreachableUri = new Uri("ws://127.0.0.1:1");

        // Act: attempt to connect with a short timeout so the test finishes quickly.
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        this.Trace("connect to unreachable endpoint");
        var exception = await ClientSocket.ConnectAsync(unreachableUri, cts.Token);

        // Assert: a non-null exception is returned (connection failure path).
        this.Trace("assert exception is non-null");
        exception.IsNotNull();

        // Assert: IsClosed reflects the pre-connect initial state (ClosedLocal, no exception).
        this.Trace("await IsClosed");
        // VSTHRD003: awaiting the fixture-owned ClientSocket.IsClosed task — not an alien task.
#pragma warning disable VSTHRD003
        var closeResult = await ClientSocket.IsClosed;
#pragma warning restore VSTHRD003

        this.Trace("assert closed local");
        closeResult.Status.Is(WebSocketCloseStatus.ClosedLocal);
        closeResult.Exception.IsDefault();

        this.Trace("done");
    }

    /// <summary>
    /// Verifies that a connect attempt to an unroutable address does not hang: cancelling the
    /// supplied token aborts the in-flight connect promptly and returns a non-null exception.
    /// This is what lets <see cref="ClientWebSocket"/>'s per-attempt ConnectTimeout make progress
    /// (cancel the stuck attempt, then reconnect) instead of getting stuck in a single attempt —
    /// the OS default connect timeout is otherwise ~75s.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task ConnectAsync_Unroutable_CancelAbortsPromptly()
    {
        this.Trace("start");

        // 192.0.2.1 is RFC 5737 TEST-NET-1: guaranteed unroutable, so the TCP SYN goes unanswered
        // and the connect blocks until it is cancelled (no fast RST like a refused local port).
        var unroutableUri = new Uri("ws://192.0.2.1:9");

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));

        this.Trace("connect to unroutable endpoint with a 500ms cancellation");
        var connectTask = ClientSocket.ConnectAsync(unroutableUri, cts.Token);

        // the connect must resolve (with an exception) well before the OS-default connect timeout;
        // WaitAsync bounds the test so a regression (cancellation not honored) fails instead of hanging
        var exception = await connectTask.WaitAsync(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken);

        this.Trace("assert exception is non-null");
        exception.IsNotNull();

        this.Trace("done");
    }

    /// <summary>
    /// Verifies that calling <see cref="IClientManagedWebSocket.ConnectAsync"/> a second time while
    /// already connected throws <see cref="InvalidOperationException"/> (the "already connected" guard).
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task ConnectAsync_AlreadyConnected_Throws()
    {
        this.Trace("start");

        this.Trace("run server");
        await using var server = RunServer(async serverSocket => await serverSocket.IsClosed);

        this.Trace("connect");
        await ConnectAsync(server, TestContext.Current.CancellationToken);

        this.Trace("connect again - must throw");
        await Wrap.It(async () =>
                await ClientSocket.ConnectAsync(server.WebSocketsUri(), TestContext.Current.CancellationToken)
            )
            .ThrowsAsync<InvalidOperationException>();

        this.Trace("done");
    }

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
        await using var server = RunServer(async serverSocket => await serverSocket.IsClosed);

        this.Trace("connect");
        await ConnectAsync(server, TestContext.Current.CancellationToken);

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
        await using var server = RunServer(async serverSocket => await serverSocket.IsClosed);

        this.Trace("connect");
        await ConnectAsync(server, TestContext.Current.CancellationToken);

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
        await using var server = RunServer(async serverSocket =>
        {
            this.Trace("disconnect server socket");
            await serverSocket.DisconnectAsync();

            this.Trace("send signal to client");
            serverTcs.SetResult();
        });

        this.Trace("connect");
        await ConnectAsync(server, TestContext.Current.CancellationToken);

        // wait for the server-side disconnect signal
        this.Trace("await server signal");
        await serverTcs.Task;

        // act — poll SendTextAsync until the close has propagated to the client socket.
        // The server-side DisconnectAsync completing is not a hard guarantee that the close
        // frame has been received and processed by the client; on slower machines / CI the
        // first send can race with the close and return Ok before the socket transitions.
        // Bounded poll within 5s catches the closed state without flaking on the race.
        this.Trace("send message and ensure it once becomes closed");
        await Expect.ToAsync(
            async () =>
            {
                var result = await SendTextAsync(message, TestContext.Current.CancellationToken);
                result.Is(WebSocketSendStatus.Closed);
            },
            ms: 5_000
        );

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
        // RunContinuationsAsynchronously: the client's await resumes off the server-handler thread.
        var serverTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        this.Trace("run server");
        await using var server = RunServer(async serverSocket =>
        {
            this.Trace("subscribe to text messages");
            serverSocket.OnTextReceived += x => serverSocket.SendTextAsync(x.ToArray(), CancellationToken.None).Await();
            this.Trace("server subscribed to text");

            this.Trace("subscribe to binary messages");
            serverSocket.OnBinaryReceived += x =>
                serverSocket.SendBinaryAsync(x.ToArray(), CancellationToken.None).Await();
            this.Trace("server subscribed to binary");

            // Signal readiness deterministically once the echo handlers are wired (they are
            // subscribed synchronously above) — no arbitrary delay needed.
            this.Trace("send signal to client");
            serverTcs.SetResult();

            this.Trace("listen server socket");
            await serverSocket.IsClosed;

            this.Trace("server socket closed");
        });

        this.Trace("connect");
        await ConnectAsync(server, TestContext.Current.CancellationToken);

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
        await using var server = RunServer(async serverSocket =>
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
        await ConnectAsync(server, TestContext.Current.CancellationToken);

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
        await ConnectAsync(server, TestContext.Current.CancellationToken);

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
        await using var server = RunServer(async serverSocket => await serverSocket.IsClosed);

        this.Trace("connect");
        var cts = new CancellationTokenSource();
        await ConnectAsync(server, cts.Token);

        this.Trace("disconnect");
        await ClientSocket.DisconnectAsync();

        // act
        this.Trace("await closed state");
        // VSTHRD003: awaiting the fixture-owned ClientSocket.IsClosed task — not an alien task.
#pragma warning disable VSTHRD003
        var result = await ClientSocket.IsClosed;
#pragma warning restore VSTHRD003

        // assert
        this.Trace("assert clean close and no exception");
        // DisconnectAsync() initiates a LOCAL close, but the listen loop can observe the peer's
        // reactive close first — the reported direction is racy at the transport level. The
        // meaningful invariant is a clean close (no exception); accept either direction.
        (result.Status is WebSocketCloseStatus.ClosedLocal or WebSocketCloseStatus.ClosedRemote).IsTrue();
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
        await using var server = RunServer(async serverSocket => await serverSocket.IsClosed);

        this.Trace("connect");
        await ConnectAsync(server, TestContext.Current.CancellationToken);

        this.Trace("disconnect client socket");
        await ClientSocket.DisconnectAsync();

        // act
        this.Trace("await closed state");
        // VSTHRD003: awaiting the fixture-owned ClientSocket.IsClosed task — not an alien task.
#pragma warning disable VSTHRD003
        var result = await ClientSocket.IsClosed;
#pragma warning restore VSTHRD003

        // assert
        this.Trace("assert clean close and no exception");
        // DisconnectAsync() initiates a LOCAL close, but the listen loop can observe the peer's
        // reactive close first — the reported direction is racy at the transport level. The
        // meaningful invariant is a clean close (no exception); accept either direction.
        (result.Status is WebSocketCloseStatus.ClosedLocal or WebSocketCloseStatus.ClosedRemote).IsTrue();
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
        await using var server = RunServer(async serverSocket => await serverSocket.DisconnectAsync());

        this.Trace("connect");
        await ConnectAsync(server, TestContext.Current.CancellationToken);

        // act
        this.Trace("await closed state");
        // VSTHRD003: awaiting the fixture-owned ClientSocket.IsClosed task — not an alien task.
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
        await using var server = RunServer(async serverSocket =>
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
        await ConnectAsync(server, TestContext.Current.CancellationToken);

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
        await using var server = RunServer(async serverSocket =>
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
        await ConnectAsync(server, TestContext.Current.CancellationToken);

        // assert
        this.Trace("assert text messages arrive");
        await Expect.ToAsync(() => _texts.IsEqual(messages));

        this.Trace("await closed state");
        // VSTHRD003: awaiting the fixture-owned ClientSocket.IsClosed task — not an alien task.
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
        await using var server = RunServer(async serverSocket =>
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
        await ConnectAsync(server, TestContext.Current.CancellationToken);

        // assert
        this.Trace("assert text messages arrive");
        await Expect.ToAsync(() => _texts.IsEqual(messages));

        this.Trace("assert binary messages arrive");
        await Expect.ToAsync(() => _binaries.IsEqual(messages));

        this.Trace("await closed state");
        // VSTHRD003: awaiting the fixture-owned ClientSocket.IsClosed task — not an alien task.
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
    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();

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

        this.Trace("done");
    }

    /// <summary>
    /// Disposes the test instance and cleans up managed WebSocket client
    /// </summary>
    /// <returns>Task representing the disposal operation</returns>
    public override async ValueTask DisposeAsync()
    {
        this.Trace("start");

        if (_clientSocket is not null)
        {
            await _clientSocket.DisconnectAsync();
            // Dispose() frees _listenCts (idempotent after DisconnectAsync on the connected path;
            // releases the constructor-created CTS on the never-connected path).
            _clientSocket.Dispose();
        }

        this.Trace("done");

        await base.DisposeAsync();
    }

    /// <summary>
    /// Runs a test server with the specified managed WebSocket handler
    /// </summary>
    /// <param name="handleWebSocket">Function to handle managed WebSocket connections</param>
    /// <returns>Disposable representing the running server</returns>
    private IServer RunServer(Func<IServerManagedWebSocket, Task> handleWebSocket)
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
    /// <param name="server">Server to connect to</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Task representing the connection operation</returns>
    private async Task ConnectAsync(IServer server, CancellationToken ct = default)
    {
        this.Trace("start");

        // assert the connect succeeded — ConnectAsync returns a non-null exception on failure;
        // ignoring it would let a failed connect proceed and surface as misleading downstream errors.
        var exception = await ClientSocket.ConnectAsync(server.WebSocketsUri(), ct);
        exception.IsDefault();

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
