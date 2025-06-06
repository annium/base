using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Testing;
using Annium.Threading.Tasks;
using Xunit;

namespace Annium.Net.WebSockets.Tests;

public class ClientServerWebSocketTests : TestBase, IAsyncLifetime
{
    private IClientWebSocket ClientSocket => _clientSocket.NotNull();
    private IClientWebSocket? _clientSocket;
    private readonly TestLog<string> _texts = new();
    private readonly TestLog<string> _binaries = new();

    public ClientServerWebSocketTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

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

    [Fact]
    public async Task Send_Canceled()
    {
        this.Trace("start");

        // arrange
        const string message = "demo";

        this.Trace("run server");
        await using var _ = RunServer(async serverSocket => await serverSocket.WhenDisconnectedAsync());

        this.Trace("connect");
        await ConnectAsync();

        // act
        this.Trace("send text");
        var result = await SendTextAsync(message, new CancellationToken(true));

        // assert
        this.Trace("assert canceled");
        result.Is(WebSocketSendStatus.Canceled);

        this.Trace("done");
    }

    [Fact]
    public async Task Send_ClientClosed()
    {
        this.Trace("start");

        // arrange
        const string message = "demo";
        var serverConnectionTcs = new TaskCompletionSource();

        this.Trace("run server");
        await using var _ = RunServer(async serverSocket =>
        {
            this.Trace("send signal to client");
            var disconnectionTask = serverSocket.WhenDisconnectedAsync();
            serverConnectionTcs.SetResult();
            await disconnectionTask;
        });

        this.Trace("connect");
        await ConnectAsync();

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

    [Fact]
    public async Task Send_ServerClosed()
    {
        this.Trace("start");

        // arrange
        const string message = "demo";

        this.Trace("run server");
        await using var _ = RunServer(serverSocket =>
        {
            this.Trace("disconnect server socket");
            serverSocket.Disconnect();

            return Task.CompletedTask;
        });

        this.Trace("connect");
        var disconnectionTask = ClientSocket.WhenDisconnectedAsync(ct: TestContext.Current.CancellationToken);
        await ConnectAsync();

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

    [Fact]
    public async Task Send_Normal()
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
            var disconnectionTask = serverSocket.WhenDisconnectedAsync();
            serverConnectionTcs.TrySetResult();
            await disconnectionTask;

            this.Trace("server socket closed");
        });

        this.Trace("connect");
        await ConnectAsync();

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
            var disconnectionTask = serverSocket.WhenDisconnectedAsync();
            serverConnectionTcs.TrySetResult();
            await disconnectionTask;

            this.Trace("server socket closed");
        });

        this.Trace("connect");
        await ConnectAsync();

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
        await ConnectAsync();

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

    [Fact]
    public async Task Listen_Normal()
    {
        this.Trace("start");

        // arrange
        var messages = Enumerable.Range(0, 3).Select(x => new string((char)x, 10)).ToArray();
        var serverStopTcs = new TaskCompletionSource();

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

#pragma warning disable VSTHRD003
            await serverStopTcs.Task;
#pragma warning restore VSTHRD003
        });

        // act
        this.Trace("connect");
        await ConnectAsync();

        // assert
        this.Trace("assert text message arrived");
        await Expect.ToAsync(() => _texts.IsEqual(messages), 1000);
        serverStopTcs.SetResult();

        this.Trace("done");
    }

    [Fact]
    public async Task Listen_SmallBuffer()
    {
        this.Trace("start");

        // arrange
        var messages = Enumerable.Range(0, 3).Select(x => new string((char)x, 1_000_000)).ToArray();
        var serverStopTcs = new TaskCompletionSource();

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

#pragma warning disable VSTHRD003
            await serverStopTcs.Task;
#pragma warning restore VSTHRD003
        });

        // act
        this.Trace("connect");
        var disconnectionTask = ClientSocket.WhenDisconnectedAsync(ct: TestContext.Current.CancellationToken);
        await ConnectAsync();

        // assert
        this.Trace("assert text message arrived");
        await Expect.ToAsync(() => _texts.IsEqual(messages), 1000);
        serverStopTcs.SetResult();

        this.Trace("disconnect");
        var result = await disconnectionTask;

        this.Trace("assert closed remote");
        result.Is(WebSocketCloseStatus.ClosedRemote);

        this.Trace("done");
    }

    [Fact]
    public async Task Listen_BothTypes()
    {
        this.Trace("start");

        // arrange
        var messages = Enumerable.Range(0, 3).Select(x => new string((char)x, 10)).ToArray();
        var serverStopTcs = new TaskCompletionSource();

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

#pragma warning disable VSTHRD003
            await serverStopTcs.Task;
#pragma warning restore VSTHRD003
        });

        // act
        this.Trace("connect");
        var disconnectionTask = ClientSocket.WhenDisconnectedAsync(ct: TestContext.Current.CancellationToken);
        await ConnectAsync();

        // assert
        this.Trace("assert text messages arrived");
        await Expect.ToAsync(() => _texts.IsEqual(messages), 1000);

        this.Trace("assert binary messages arrived");
        await Expect.ToAsync(() => _binaries.IsEqual(messages), 1000);
        serverStopTcs.SetResult();

        this.Trace("disconnect");
        var result = await disconnectionTask;

        this.Trace("assert closed remote");
        result.Is(WebSocketCloseStatus.ClosedRemote);

        this.Trace("done");
    }

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
        await using var _ = RunServer(async serverSocket =>
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
        await ConnectAsync();

        // assert
        this.Trace("wait for {messagesCount} messages", messages.Length);
        await Expect.ToAsync(() => _texts.IsEqual(messages), 1000);

        this.Trace("send signal to stop server");
        serverStopTcs.SetResult();

        this.Trace("disconnect");
        await DisconnectAsync();

        this.Trace("done");
    }

    public ValueTask InitializeAsync()
    {
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
        ClientSocket.OnError += e => Assert.Fail($"Exception occured: {e}");

        this.Trace("done");

        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        this.Trace("start");

        _clientSocket?.Disconnect();

        this.Trace("done");

        return ValueTask.CompletedTask;
    }

    private IAsyncDisposable RunServer(Func<ServerWebSocket, Task> handleWebSocket)
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

    private async Task ConnectAsync()
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

        ClientSocket.Connect(ServerUri);

        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(10));

        this.Trace("done");
    }

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

        await tcs.Task;

        this.Trace("done");
    }

    private async Task<WebSocketSendStatus> SendTextAsync(string text, CancellationToken ct = default)
    {
        this.Trace("start");

        var result = await ClientSocket.SendTextAsync(text, ct);

        this.Trace("done");

        return result;
    }

    private async Task<WebSocketSendStatus> SendBinaryAsync(string data, CancellationToken ct = default)
    {
        this.Trace("start");

        var result = await ClientSocket.SendBinaryAsync(data, ct);

        this.Trace("done");

        return result;
    }
}
