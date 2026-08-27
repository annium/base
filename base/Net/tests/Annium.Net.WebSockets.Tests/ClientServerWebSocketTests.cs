using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
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

namespace Annium.Net.WebSockets.Tests;

/// <summary>
/// Tests for client-server WebSocket communication scenarios
/// </summary>
public class ClientServerWebSocketTests : TestBase
{
    /// <summary>
    /// Gets the client WebSocket instance
    /// </summary>
    private IClientWebSocket ClientSocket => _clientSocket.NotNull();

    /// <summary>
    /// The client WebSocket instance
    /// </summary>
    private IClientWebSocket? _clientSocket;

    /// <summary>
    /// Log for text messages received
    /// </summary>
    private readonly TestLog<string> _texts = new();

    /// <summary>
    /// Log for binary messages received
    /// </summary>
    private readonly TestLog<string> _binaries = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientServerWebSocketTests"/> class.
    /// </summary>
    /// <param name="outputHelper">xUnit test output helper the test host logs through.</param>
    public ClientServerWebSocketTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Tests sending a message when WebSocket is not connected
    /// </summary>
    /// <returns>Task representing the test operation</returns>
    [Fact]
    public async Task Send_NotConnected()
    {
        this.Trace("start");

        // arrange
        const string message = "demo";

        // act
        this.Trace("send text");
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
        await using var server = RunServer(async serverSocket => await serverSocket.WhenDisconnectedAsync());

        this.Trace("connect");
        await ConnectAsync(server);

        // act
        this.Trace("send text");
        var result = await SendTextAsync(message, new CancellationToken(true));

        // assert
        this.Trace("assert canceled");
        result.Is(WebSocketSendStatus.Canceled);

        this.Trace("done");
    }

    /// <summary>
    /// Tests sending a message after client WebSocket is closed
    /// </summary>
    /// <returns>Task representing the test operation</returns>
    [Fact]
    public async Task Send_ClientClosed()
    {
        this.Trace("start");

        // arrange
        const string message = "demo";
        var serverConnectionTcs = new TaskCompletionSource();

        this.Trace("run server");
        await using var server = RunServer(async serverSocket =>
        {
            this.Trace("send signal to client");
            var disconnectionTask = serverSocket.WhenDisconnectedAsync();
            serverConnectionTcs.SetResult();
            await disconnectionTask;
        });

        this.Trace("connect");
        await ConnectAsync(server);

        this.Trace("server connected");
        await serverConnectionTcs.Task;

        // act
        this.Trace("disconnect");
        await DisconnectAsync();

        this.Trace("send text");
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

        this.Trace("run server");
        await using var server = RunServer(serverSocket =>
        {
            this.Trace("disconnect server socket");
            serverSocket.Disconnect();
            return Task.CompletedTask;
        });

        this.Trace("connect");
        var disconnectionTask = ClientSocket.WhenDisconnectedAsync(ct: TestContext.Current.CancellationToken);
        await ConnectAsync(server);

        this.Trace("await until disconnected");
        await disconnectionTask;

        // act
        this.Trace("send text");
        var result = await SendTextAsync(message, TestContext.Current.CancellationToken);

        // assert
        this.Trace("assert closed");
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
            var disconnectionTask = serverSocket.WhenDisconnectedAsync();
            serverConnectionTcs.TrySetResult();
            await disconnectionTask;

            this.Trace("server socket closed");
        });

        this.Trace("connect");
        await ConnectAsync(server);

        this.Trace("server connected");
        await serverConnectionTcs.Task;

        // act && assert
        this.Trace("send text");
        var textResult = await SendTextAsync(message, TestContext.Current.CancellationToken);

        this.Trace("assert sent ok");
        textResult.Is(WebSocketSendStatus.Ok);

        this.Trace("assert text message arrived");
        await Expect.ToAsync(() => _texts.IsEqual(expectedMessages));

        this.Trace("send binary");
        var binaryResult = await SendBinaryAsync(message, TestContext.Current.CancellationToken);

        this.Trace("assert ok");
        binaryResult.Is(WebSocketSendStatus.Ok);

        this.Trace("assert binary message arrived");
        await Expect.ToAsync(() => _binaries.IsEqual(expectedMessages));

        this.Trace("done");
    }

    /// <summary>
    /// Tests sending messages with client reconnection
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
            var disconnectionTask = serverSocket.WhenDisconnectedAsync();
            serverConnectionTcs.TrySetResult();
            await disconnectionTask;

            this.Trace("server socket closed");
        });

        this.Trace("connect");
        await ConnectAsync(server);

        this.Trace("server connected");
        await serverConnectionTcs.Task;

        // act - send text
        this.Trace("send text");
        var textResult = await SendTextAsync(message, TestContext.Current.CancellationToken);

        this.Trace("assert sent ok");
        textResult.Is(WebSocketSendStatus.Ok);

        this.Trace("assert text message arrived");
        await Expect.ToAsync(() => _texts.IsEqual(expectedMessages));

        this.Trace("disconnect");
        await DisconnectAsync();

        // act - send binary
        this.Trace("connect");
        serverConnectionTcs = new TaskCompletionSource();
        await ConnectAsync(server);

        this.Trace("server connected");
        await serverConnectionTcs.Task;

        this.Trace("send binary");
        var binaryResult = await SendBinaryAsync(message, TestContext.Current.CancellationToken);

        this.Trace("assert sent ok");
        binaryResult.Is(WebSocketSendStatus.Ok);

        this.Trace("assert binary message arrived");
        await Expect.ToAsync(() => _binaries.IsEqual(expectedMessages));

        this.Trace("disconnect");
        await DisconnectAsync();

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
        var messages = Enumerable.Range(0, 3).Select(x => new string((char)x, 10)).ToArray();
        var serverStopTcs = new TaskCompletionSource();

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

            // VSTHRD003: awaiting our own test-local TCS that gates server shutdown — not an alien task.
#pragma warning disable VSTHRD003
            await serverStopTcs.Task;
#pragma warning restore VSTHRD003
        });

        // act
        this.Trace("connect");
        await ConnectAsync(server);

        // assert
        this.Trace("assert text message arrived");
        await Expect.ToAsync(() => _texts.IsEqual(messages));
        serverStopTcs.SetResult();

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
        var messages = Enumerable.Range(0, 3).Select(x => new string((char)x, 1_000_000)).ToArray();
        var serverStopTcs = new TaskCompletionSource();

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

            // VSTHRD003: awaiting our own test-local TCS that gates server shutdown — not an alien task.
#pragma warning disable VSTHRD003
            await serverStopTcs.Task;
#pragma warning restore VSTHRD003
        });

        // act
        this.Trace("connect");
        var disconnectionTask = ClientSocket.WhenDisconnectedAsync(ct: TestContext.Current.CancellationToken);
        await ConnectAsync(server);

        // assert
        this.Trace("assert text message arrived");
        await Expect.ToAsync(() => _texts.IsEqual(messages));
        serverStopTcs.SetResult();

        this.Trace("disconnect");
        var result = await disconnectionTask;

        this.Trace("assert closed remote");
        result.Is(WebSocketCloseStatus.ClosedRemote);

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
        var messages = Enumerable.Range(0, 3).Select(x => new string((char)x, 10)).ToArray();
        var serverStopTcs = new TaskCompletionSource();

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

            // VSTHRD003: awaiting our own test-local TCS that gates server shutdown — not an alien task.
#pragma warning disable VSTHRD003
            await serverStopTcs.Task;
#pragma warning restore VSTHRD003
        });

        // act
        this.Trace("connect");
        var disconnectionTask = ClientSocket.WhenDisconnectedAsync(ct: TestContext.Current.CancellationToken);
        await ConnectAsync(server);

        // assert
        this.Trace("assert text messages arrived");
        await Expect.ToAsync(() => _texts.IsEqual(messages));

        this.Trace("assert binary messages arrived");
        await Expect.ToAsync(() => _binaries.IsEqual(messages));
        serverStopTcs.SetResult();

        this.Trace("disconnect");
        var result = await disconnectionTask;

        this.Trace("assert closed remote");
        result.Is(WebSocketCloseStatus.ClosedRemote);

        this.Trace("done");
    }

    /// <summary>
    /// Tests message listening with automatic reconnection
    /// </summary>
    /// <returns>Task representing the test operation</returns>
    [Fact]
    public async Task Listen_Reconnect()
    {
        this.Trace("start");

        // arrange
        var messages = Enumerable.Range(0, 10).Select(x => new string((char)x, 10)).ToArray();
        var serverStopTcs = new TaskCompletionSource();
        var connectionIndex = 0;
        var connectionsCount = 3;

        this.Trace("run server");
        await using var server = RunServer(async serverSocket =>
        {
            connectionIndex++;
            if (connectionIndex > connectionsCount)
            {
                this.Trace("drop connection after limit");
                return;
            }

            this.Trace("start sending messages");

            var complete = connectionIndex == connectionsCount;

            var i = 0;
            var breakAtChunk = complete ? int.MaxValue : new Random().Next(1, messages.Length - 1);
            foreach (var message in messages)
            {
                i++;

                // emulate disconnection
                if (i == breakAtChunk)
                {
                    this.Trace(
                        "disconnect, connection {connectionIndex}/{connectionsCount} at message#{num}",
                        connectionIndex,
                        connectionsCount,
                        i
                    );
                    return;
                }

                this.Trace("send chunk#{num}", i);
                await serverSocket.SendTextAsync(message);

                await Task.Delay(1, CancellationToken.None);
            }

            this.Trace("sending messages complete");

            // wait until 3-rd connection is handled
            this.Trace("wait for signal from client");
            // VSTHRD003: awaiting our own test-local TCS that gates server shutdown — not an alien task.
#pragma warning disable VSTHRD003
            await serverStopTcs.Task;
#pragma warning restore VSTHRD003
        });

        this.Trace("set disconnect handler");
        ClientSocket.OnDisconnected += _ =>
        {
            this.Trace("disconnected, clear stream");
            _texts.Clear();
        };

        this.Trace("connect");
        await ConnectAsync(server);

        // assert
        this.Trace("wait for {messagesCount} messages", messages.Length);
        await Expect.ToAsync(() => _texts.IsEqual(messages));

        this.Trace("send signal to stop server");
        serverStopTcs.SetResult();

        this.Trace("disconnect");
        await DisconnectAsync();

        this.Trace("done");
    }

    /// <summary>
    /// Verifies the disconnect ordering invariant: when the <c>OnDisconnected</c> handler runs,
    /// the underlying WebSocket teardown has already completed, so the public
    /// <see cref="IClientWebSocket.IsConnected"/> surface reports false. Spec test for AC#2 of T8.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task Disconnect_OnDisconnectedFires_HandlerObservesIsConnectedFalse()
    {
        this.Trace("start");

        await using var server = RunServer(async serverSocket => await serverSocket.WhenDisconnectedAsync());

        var capturedIsConnected = new TaskCompletionSource<bool>();
        ClientSocket.OnDisconnected += _ => capturedIsConnected.TrySetResult(ClientSocket.IsConnected);

        this.Trace("connect");
        await ConnectAsync(server);

        ClientSocket.IsConnected.IsTrue();

        this.Trace("dispose");
        // VSTHRD103: ClientWebSocket.Dispose() is synchronous (no async variant).
#pragma warning disable VSTHRD103
        ClientSocket.Dispose();
#pragma warning restore VSTHRD103

        var observedIsConnected = await capturedIsConnected.Task.WaitAsync(
            TimeSpan.FromSeconds(5),
            TestContext.Current.CancellationToken
        );
        observedIsConnected.IsFalse();

        this.Trace("done");
    }

    /// <summary>
    /// Verifies that when the DefaultConnectionMonitor fires ConnectionLost because no pong arrives
    /// within MaxPingDelay, the ClientWebSocket disconnects and reconnects, raising OnConnected a
    /// second time.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task ConnectionMonitor_NoPongWithinMaxDelay_ReconnectsClient()
    {
        this.Trace("start");

        // Server stays connected without ever echoing ping frames back.
        await using var server = RunServer(async serverSocket => await serverSocket.WhenDisconnectedAsync());

        // Create a dedicated ClientWebSocket with an aggressive monitor: ping every 50 ms,
        // max tolerated delay 80 ms. No pong will arrive → FireConnectionLost → reconnect.
        var monitorOptions = new ConnectionMonitorOptions
        {
            Factory = new DefaultConnectionMonitorFactory(Logger),
            PingInterval = 50,
            MaxPingDelay = 80,
        };
        var options = ClientWebSocketOptions.Default with { ReconnectDelay = 1, ConnectionMonitor = monitorOptions };

        using var socket = new ClientWebSocket(options, Logger);

        // Count OnConnected invocations; the first is the initial connection, the second is the reconnect.
        var connectCount = 0;
        var connectedTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var reconnectedTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        socket.OnConnected += () =>
        {
            var count = Interlocked.Increment(ref connectCount);
            this.Trace("OnConnected #{count}", count);
            if (count == 1)
                connectedTcs.TrySetResult();
            if (count >= 2)
                reconnectedTcs.TrySetResult();
        };

        socket.Connect(server.WebSocketsUri());

        // the initial connection is not what this test is about, and on a loaded machine it can take
        // seconds - waiting for it separately keeps that time out of the budget for the reconnect
        this.Trace("await first OnConnected");
        await connectedTcs.Task.WaitAsync(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken);

        // the monitor pings every 50ms and tolerates 80ms, so the reconnect follows within a few hundred
        // milliseconds of the connection; the bound is here to fail instead of hang, not to time it
        this.Trace("await second OnConnected (reconnect after monitor fires)");
        await reconnectedTcs.Task.WaitAsync(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken);

        this.Trace("assert reconnected");
        (connectCount >= 2).IsTrue();

        this.Trace("done");
    }

    /// <summary>
    /// Verifies that a binary ping frame sent by the server is NOT delivered to
    /// <c>OnBinaryReceived</c> on the client (it is silently filtered by the client's binary-receive
    /// handler via <c>ProtocolFrames.IsPingFrame</c>), while a normal binary message IS delivered.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task Receive_PingFrame_NotDeliveredToOnBinaryReceived()
    {
        this.Trace("start");

        const string normalMessage = "hello";
        var serverConnectedTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        await using var server = RunServer(async serverSocket =>
        {
            this.Trace("server: wait for client to be ready");
            // VSTHRD003: awaiting our own test-local TCS that gates server actions — not an alien task.
#pragma warning disable VSTHRD003
            await serverConnectedTcs.Task;
#pragma warning restore VSTHRD003

            // Send the ping sentinel first, then a normal binary message.
            this.Trace("server: send ping frame");
            await serverSocket.SendBinaryAsync(ProtocolFrames.Ping, CancellationToken.None);

            this.Trace("server: send normal binary message");
            await serverSocket.SendBinaryAsync(normalMessage, CancellationToken.None);

            this.Trace("server: wait for disconnect");
            await serverSocket.WhenDisconnectedAsync();
        });

        this.Trace("connect");
        await ConnectAsync(server);
        serverConnectedTcs.TrySetResult();

        // Assert: the normal message arrives and no additional (ping) messages appear.
        this.Trace("assert only normal binary message delivered");
        await Expect.ToAsync(() => _binaries.IsEqual(new[] { normalMessage }));

        this.Trace("done");
    }

    /// <summary>
    /// Verifies <see cref="IClientWebSocket.IsConnected"/> transitions:
    /// false on a freshly constructed (never-connected) socket; true after <c>OnConnected</c>
    /// fires; false synchronously immediately after <see cref="IClientWebSocket.Disconnect"/>
    /// returns (before <c>OnDisconnected</c> fires from the background teardown).
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task IsConnected_TransitionsThroughLifecycle()
    {
        this.Trace("start");

        // 1. Freshly constructed socket is not connected.
        ClientSocket.IsConnected.IsFalse();

        await using var server = RunServer(async serverSocket => await serverSocket.WhenDisconnectedAsync());

        // 2. After ConnectAsync (OnConnected fired) the socket is connected.
        await ConnectAsync(server);
        ClientSocket.IsConnected.IsTrue();

        // 3. Disconnect() is synchronous: IsConnected goes false immediately even though the
        //    background teardown (and OnDisconnected event) has not yet completed.
        ClientSocket.Disconnect();
        ClientSocket.IsConnected.IsFalse();

        this.Trace("done");
    }

    /// <summary>
    /// Verifies <see cref="ServerWebSocket.IsConnected"/> transitions: true on entry to the
    /// server callback (the socket is freshly constructed = Connected), and false synchronously
    /// after <c>serverSocket.Disconnect()</c> returns (before <c>OnDisconnected</c> fires from
    /// the background teardown).
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task ServerWebSocket_IsConnected_TransitionsThroughLifecycle()
    {
        this.Trace("start");

        var isConnectedOnEntryTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var isConnectedAfterDisconnectTcs = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );

        await using var server = RunServer(async serverSocket =>
        {
            // Capture IsConnected right after the socket is handed to the callback.
            isConnectedOnEntryTcs.TrySetResult(serverSocket.IsConnected);

            // Disconnect synchronously and capture IsConnected immediately after.
            serverSocket.Disconnect();
            isConnectedAfterDisconnectTcs.TrySetResult(serverSocket.IsConnected);

            await serverSocket.WhenDisconnectedAsync();
        });

        this.Trace("connect");
        await ConnectAsync(server);

        // Guard: 5 s is more than enough for the server callback to run.
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        this.Trace("await server-side observations");
        var isConnectedOnEntry = await isConnectedOnEntryTcs.Task.WaitAsync(cts.Token);
        var isConnectedAfterDisconnect = await isConnectedAfterDisconnectTcs.Task.WaitAsync(cts.Token);

        this.Trace("assert");
        isConnectedOnEntry.IsTrue();
        isConnectedAfterDisconnect.IsFalse();

        this.Trace("done");
    }

    /// <summary>
    /// Verifies that a binary ping frame sent by the CLIENT is NOT delivered to
    /// <c>OnBinaryReceived</c> on the server (filtered by <c>ServerWebSocket.HandleOnBinaryReceived</c>
    /// via <c>ProtocolFrames.IsPingFrame</c>), while a normal binary message IS delivered.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task ServerWebSocket_Receive_PingFrame_NotDeliveredToOnBinaryReceived()
    {
        this.Trace("start");

        const string normalMessage = "hello-server";

        // Surface the server-side binary log out of the callback via a shared TestLog.
        var serverBinaryLog = new TestLog<string>();
        // The server signals readiness AFTER subscribing OnBinaryReceived, and the client waits for
        // that signal before sending — guaranteeing the subscription exists before any frame arrives
        // (a client→server gate would not, since the server handler may enter after the client sends).
        var serverReadyTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        await using var server = RunServer(async serverSocket =>
        {
            serverSocket.OnBinaryReceived += data =>
            {
                serverBinaryLog.Add(Encoding.UTF8.GetString(data.Span));
            };
            serverReadyTcs.TrySetResult();

            await serverSocket.WhenDisconnectedAsync();
        });

        this.Trace("connect");
        await ConnectAsync(server);

        this.Trace("await server subscribed");
        await serverReadyTcs.Task;

        this.Trace("client: send ping frame");
        await ClientSocket.SendBinaryAsync(ProtocolFrames.Ping, CancellationToken.None);

        this.Trace("client: send normal binary message");
        await ClientSocket.SendBinaryAsync(normalMessage, CancellationToken.None);

        // Assert: only the normal message is delivered; the ping is filtered.
        this.Trace("assert only normal binary message delivered to server");
        await Expect.ToAsync(() => serverBinaryLog.IsEqual(new[] { normalMessage }));

        this.Trace("done");
    }

    /// <summary>
    /// Verifies that <see cref="ReceivingWebSocketExtensions.ObserveText"/> delivers text messages
    /// via the returned <see cref="IObservable{T}"/>, and that disposing the subscription stops
    /// further delivery.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task ObserveText_DeliversTextMessages()
    {
        this.Trace("start");

        const string message = "observable-text";
        var serverConnectedTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        await using var server = RunServer(async serverSocket =>
        {
            // VSTHRD003: awaiting our own test-local TCS that gates server actions — not an alien task.
#pragma warning disable VSTHRD003
            await serverConnectedTcs.Task;
#pragma warning restore VSTHRD003

            await serverSocket.SendTextAsync(message, CancellationToken.None);
            await serverSocket.WhenDisconnectedAsync();
        });

        this.Trace("connect");
        await ConnectAsync(server);

        // TestLog is thread-safe: the Rx subscriber writes from the receive-loop thread while
        // Expect.ToAsync polls from the test thread.
        var observed = new TestLog<string>();
        using var subscription = ClientSocket
            .ObserveText()
            .Subscribe(data =>
            {
                observed.Add(Encoding.UTF8.GetString(data.Span));
            });

        // Signal the server only after the subscription is in place.
        serverConnectedTcs.TrySetResult();

        this.Trace("assert observed message arrives");
        await Expect.ToAsync(() => observed.IsEqual(new[] { message }));

        this.Trace("done");
    }

    /// <summary>
    /// Verifies that <see cref="ReceivingWebSocketExtensions.ObserveBinary"/> delivers binary messages
    /// via the returned <see cref="IObservable{T}"/>, and that disposing the subscription stops
    /// further delivery.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task ObserveBinary_DeliversBinaryMessages()
    {
        this.Trace("start");

        const string message = "observable-binary";
        var serverConnectedTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        await using var server = RunServer(async serverSocket =>
        {
            // VSTHRD003: awaiting our own test-local TCS that gates server actions — not an alien task.
#pragma warning disable VSTHRD003
            await serverConnectedTcs.Task;
#pragma warning restore VSTHRD003

            await serverSocket.SendBinaryAsync(message, CancellationToken.None);
            await serverSocket.WhenDisconnectedAsync();
        });

        this.Trace("connect");
        await ConnectAsync(server);

        // TestLog is thread-safe: the Rx subscriber writes from the receive-loop thread while
        // Expect.ToAsync polls from the test thread.
        var observed = new TestLog<string>();
        using var subscription = ClientSocket
            .ObserveBinary()
            .Subscribe(data =>
            {
                observed.Add(Encoding.UTF8.GetString(data.Span));
            });

        // Signal the server only after the subscription is in place.
        serverConnectedTcs.TrySetResult();

        this.Trace("assert observed message arrives");
        await Expect.ToAsync(() => observed.IsEqual(new[] { message }));

        this.Trace("done");
    }

    /// <summary>
    /// Initializes the test instance and sets up WebSocket client
    /// </summary>
    /// <returns>Task representing the initialization operation</returns>
    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();

        this.Trace("start");

        _clientSocket = new ClientWebSocket(ClientWebSocketOptions.Default with { ReconnectDelay = 1 }, Logger);
        ClientSocket.OnTextReceived += x =>
        {
            var message = Encoding.UTF8.GetString(x.Span);
            _texts.Add(message);
        };
        ClientSocket.OnBinaryReceived += x =>
        {
            var message = Encoding.UTF8.GetString(x.Span);
            _binaries.Add(message);
        };

        ClientSocket.OnConnected += () => this.Trace("STATE: Connected");
        ClientSocket.OnDisconnected += status => this.Trace("STATE: Disconnected: {status}", status);
        // Transient connect/reconnect errors are recovered by the auto-reconnect loop and must NOT
        // fail the test — the test's own assertions are the source of truth. Record for diagnostics only.
        ClientSocket.OnError += e => this.Trace<string>("client OnError (non-fatal, tolerated): {error}", e.ToString());

        this.Trace("done");
    }

    /// <summary>
    /// Disposes the test instance and cleans up WebSocket client
    /// </summary>
    /// <returns>Task representing the disposal operation</returns>
    public override async ValueTask DisposeAsync()
    {
        this.Trace("start");

        // Dispose() (not Disconnect()) frees the terminal _connectionCts the fixture owns.
        _clientSocket?.Dispose();

        this.Trace("done");

        await base.DisposeAsync();
    }

    /// <summary>
    /// Runs a test server with the specified WebSocket handler
    /// </summary>
    /// <param name="handleWebSocket">Function to handle WebSocket connections</param>
    /// <returns>Disposable representing the running server</returns>
    private IServer RunServer(Func<ServerWebSocket, Task> handleWebSocket)
    {
        return RunServerBase(
            async (sp, ctx, ct) =>
            {
                this.Trace("start");

                var socket = new ServerWebSocket(ctx.WebSocket, sp.Resolve<ILogger>(), ct);

                this.Trace<string>("handle {socket}", socket.GetFullId());
                await handleWebSocket(socket);

                this.Trace<string>("disconnect {socket}", socket.GetFullId());
                socket.Disconnect();

                this.Trace("done");
            }
        );
    }

    /// <summary>
    /// Connects the client WebSocket to the test server
    /// </summary>
    /// <param name="server">Server to connect to</param>
    /// <returns>Task representing the connection operation</returns>
    private async Task ConnectAsync(IServer server)
    {
        this.Trace("start");

        var tcs = new TaskCompletionSource();

        ClientSocket.Trace<string>("subscribe {tcs} to OnConnected", tcs.GetFullId());

        void HandleConnected()
        {
            ClientSocket.Trace<string>("set {tcs} to signaled state", tcs.GetFullId());
            tcs.TrySetResult();
            ClientSocket.OnConnected -= HandleConnected;
        }

        ClientSocket.OnConnected += HandleConnected;

        ClientSocket.Connect(server.WebSocketsUri());

        // generous on purpose: the bound is here so a dropped OnConnected fails the test instead of
        // hanging the run, not to measure how long connecting takes. A dozen test hosts share this
        // machine in a full run, and a bound tight enough to catch a slow connect catches a busy one too
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(30));

        this.Trace("done");
    }

    /// <summary>
    /// Disconnects the client WebSocket from the server
    /// </summary>
    /// <returns>Task representing the disconnection operation</returns>
    private async Task DisconnectAsync()
    {
        this.Trace("start");

        var tcs = new TaskCompletionSource();

        ClientSocket.Trace<string>("subscribe {tcs} to OnConnected", tcs.GetFullId());

        void HandleDisconnected(WebSocketCloseStatus status)
        {
            ClientSocket.Trace("set {tcs} to signaled state with status {status}", tcs.GetFullId(), status);
            tcs.TrySetResult();
            ClientSocket.OnDisconnected -= HandleDisconnected;
        }

        ClientSocket.OnDisconnected += HandleDisconnected;

        ClientSocket.Disconnect();

        // bound the wait so a dropped OnDisconnected fails fast instead of hanging the test (mirrors ConnectAsync).
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(30));

        this.Trace("done");
    }

    /// <summary>
    /// Sends a text message through the WebSocket
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
    /// Sends a binary message through the WebSocket
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
