using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Net.WebSockets.Internal;
using Annium.Testing;
using Annium.Testing.Assertions;
using Xunit;
using Xunit.Abstractions;
using NativeClientWebSocket = System.Net.WebSockets.ClientWebSocket;

namespace Annium.Net.WebSockets.Tests.Internal;

public class ManagedWebSocketTests : TestBase, IAsyncLifetime
{
    private NativeClientWebSocket _clientSocket = default!;
    private ManagedWebSocket _managedSocket = default!;
    private readonly ConcurrentQueue<string> _texts = new();
    private readonly ConcurrentQueue<byte[]> _binaries = new();

    public ManagedWebSocketTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task Send_NotConnected()
    {
        this.Trace("start");

        // arrange
        const string message = "demo";

        // act
        this.Trace("send message");
        var result = await SendTextAsync(message);

        // assert
        this.Trace("assert status is closed");
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
        await using var _ = RunServer(async (serverSocket, ct) => await serverSocket.ListenAsync(ct));

        this.Trace("connect and start listening");
        await ConnectAndStartListenAsync();

        // act
        this.Trace("send message");
        var result = await SendTextAsync(message, new CancellationToken(true));

        // assert
        this.Trace("assert status is canceled");
        result.Is(WebSocketSendStatus.Canceled);

        this.Trace("done");
    }

    [Fact]
    public async Task Send_ClientClosed()
    {
        this.Trace("start");

        // arrange
        const string message = "demo";

        this.Trace("run server");
        await using var _ = RunServer(async (serverSocket, ct) => await serverSocket.ListenAsync(ct));

        this.Trace("connect and start listening");
        await ConnectAndStartListenAsync();

        // act
        this.Trace("close output");
        _clientSocket.CloseOutputAsync(System.Net.WebSockets.WebSocketCloseStatus.Empty, string.Empty, CancellationToken.None).GetAwaiter();

        this.Trace("send message");
        var result = await SendTextAsync(message);

        // assert
        this.Trace("assert status is closed");
        result.Is(WebSocketSendStatus.Closed);

        this.Trace("done");
    }

    [Fact]
    public async Task Send_ServerClosed()
    {
        this.Trace("start");

        // arrange
        const string message = "demo";
        var serverTcs = new TaskCompletionSource();

        this.Trace("run server");
        await using var _ = RunServerBase(async (_, ctx, _) =>
        {
            await ctx.WebSocket.CloseOutputAsync(System.Net.WebSockets.WebSocketCloseStatus.Empty, string.Empty, default);
            Task.Delay(10, CancellationToken.None)
                .ContinueWith(_ => serverTcs.SetResult(), CancellationToken.None)
                .GetAwaiter();
        });

        this.Trace("connect and start listening");
        await ConnectAndStartListenAsync();

        // delay to let server close connection
        this.Trace("await server signal");
        await serverTcs.Task;

        // act
        this.Trace("send message");
        var result = await SendTextAsync(message);

        // assert
        this.Trace("assert status is closed");
        result.Is(WebSocketSendStatus.Closed);

        this.Trace("done");
    }

    [Fact]
    public async Task Send_ClientAborted()
    {
        this.Trace("start");

        // arrange
        const string message = "demo";

        this.Trace("run server");
        await using var _ = RunServer(async (serverSocket, ct) => await serverSocket.ListenAsync(ct));

        this.Trace("connect and start listening");
        await ConnectAndStartListenAsync();

        // act
        this.Trace("abort client socket");
        _clientSocket.Abort();

        this.Trace("send message");
        var result = await SendTextAsync(message);

        // assert
        this.Trace("assert status is closed");
        result.Is(WebSocketSendStatus.Closed);

        this.Trace("done");
    }

    [Fact]
    public async Task Send_ServerAborted()
    {
        this.Trace("start");

        // arrange
        const string message = "demo";
        var serverTcs = new TaskCompletionSource();

        this.Trace("run server");
        await using var _ = RunServerBase(async (_, ctx, _) =>
        {
            this.Trace("abort server socket");
            ctx.WebSocket.Abort();

            await Task.CompletedTask;

            Task.Delay(10, CancellationToken.None)
                .ContinueWith(_ =>
                {
                    this.Trace("send signal to client");
                    serverTcs.SetResult();
                }, CancellationToken.None)
                .GetAwaiter();
        });

        this.Trace("connect and start listening");
        await ConnectAndStartListenAsync();

        // act
        // delay to let server close connection
        this.Trace("await server signal");
        await serverTcs.Task;

        this.Trace("send message");
        var result = await SendTextAsync(message);

        // assert
        this.Trace("assert status is closed");
        result.Is(WebSocketSendStatus.Closed);

        this.Trace("done");
    }

    [Fact]
    public async Task Send_Normal()
    {
        this.Trace("start");

        // arrange
        const string text = "demo";
        var binary = Encoding.UTF8.GetBytes(text);
        var serverTcs = new TaskCompletionSource();

        this.Trace("run server");
        await using var _ = RunServer(async (serverSocket, ct) =>
        {
            this.Trace("subscribe to text messages");
            serverSocket.TextReceived += x => serverSocket
                .SendTextAsync(x.ToArray(), CancellationToken.None)
                .GetAwaiter()
                .GetResult();

            this.Trace("subscribe to binary messages");
            serverSocket.BinaryReceived += x => serverSocket
                .SendBinaryAsync(x.ToArray(), CancellationToken.None)
                .GetAwaiter()
                .GetResult();

            Task.Delay(10, CancellationToken.None)
                .ContinueWith(_ =>
                {
                    this.Trace("send signal to client");
                    serverTcs.SetResult();
                }, CancellationToken.None)
                .GetAwaiter();

            this.Trace("listen server socket");
            await serverSocket.ListenAsync(ct);

            this.Trace("server socket closed");
        });

        this.Trace("connect and start listening");
        await ConnectAndStartListenAsync();

        // delay to let server close connection
        this.Trace("await server signal");
        await serverTcs.Task;

        // act
        this.Trace("send text message");
        var textResult = await SendTextAsync(text);

        this.Trace("send binary message");
        var binaryResult = await SendBinaryAsync(binary);

        // assert
        this.Trace("assert text result is ok");
        textResult.Is(WebSocketSendStatus.Ok);

        this.Trace("assert binary result is ok");
        binaryResult.Is(WebSocketSendStatus.Ok);

        this.Trace("assert text messages arrive");
        var expectedTexts = new[] { text };
        await Expect.To(() => _texts.IsEqual(expectedTexts));

        this.Trace("assert binary messages arrive");
        var expectedBinaries = new[] { binary };
        await Expect.To(() => _binaries.IsEqual(expectedBinaries));

        this.Trace("done");
    }

    [Fact]
    public async Task Listen_Canceled()
    {
        this.Trace("start");

        // arrange
        this.Trace("run server");
        await using var _ = RunServer(async (serverSocket, ct) => await serverSocket.ListenAsync(ct));

        this.Trace("connect");
        await ConnectAsync();

        // act
        this.Trace("listen");
        var result = await ListenAsync(new CancellationToken(true));

        // assert
        this.Trace("assert status is closed local and no exception");
        result.Status.Is(WebSocketCloseStatus.ClosedLocal);
        result.Exception.IsDefault();

        this.Trace("done");
    }

    [Fact]
    public async Task Listen_ClientClosed()
    {
        this.Trace("start");

        // arrange
        this.Trace("run server");
        await using var _ = RunServer(async (serverSocket, ct) => await serverSocket.ListenAsync(ct));

        this.Trace("connect");
        await ConnectAsync();

        this.Trace("close client socket");
        await _clientSocket.CloseOutputAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, string.Empty, default);

        // act
        this.Trace("listen");
        var result = await ListenAsync();

        // assert
        this.Trace("assert status is closed local and no exception");
        result.Status.Is(WebSocketCloseStatus.ClosedLocal);
        result.Exception.IsDefault();

        this.Trace("done");
    }

    [Fact]
    public async Task Listen_ServerClosed()
    {
        this.Trace("start");

        // arrange
        this.Trace("run server");
        await using var _ = RunServerBase(async (_, ctx, _) => await ctx.WebSocket.CloseOutputAsync(System.Net.WebSockets.WebSocketCloseStatus.Empty, string.Empty, default));

        this.Trace("connect");
        await ConnectAsync();

        // act
        this.Trace("listen");
        var result = await ListenAsync();

        // assert
        this.Trace("assert status is closed remote and no exception");
        result.Status.Is(WebSocketCloseStatus.ClosedRemote);
        result.Exception.IsDefault();

        this.Trace("done");
    }

    [Fact]
    public async Task Listen_ClientAborted()
    {
        this.Trace("start");

        // arrange
        this.Trace("run server");
        await using var _ = RunServer(async (serverSocket, ct) => await serverSocket.ListenAsync(ct));

        this.Trace("connect");
        await ConnectAsync();

        this.Trace("listen detached");
        var listenTask = ListenAsync();

        // act
        this.Trace("abort client socket");
        _clientSocket.Abort();

        this.Trace("await listen task");
        var result = await listenTask;

        // assert
        this.Trace("assert status is closed local and no exception");
        result.Status.Is(WebSocketCloseStatus.ClosedLocal);
        result.Exception.IsDefault();

        this.Trace("done");
    }

    [Fact]
    public async Task Listen_ServerAborted()
    {
        this.Trace("start");

        // arrange
        var serverTcs = new TaskCompletionSource();

        this.Trace("run server");
        await using var _ = RunServerBase(async (_, ctx, _) =>
        {
            this.Trace("abort server socket");
            ctx.WebSocket.Abort();

            await Task.CompletedTask;

            Task.Delay(10, CancellationToken.None)
                .ContinueWith(_ =>
                {
                    this.Trace("send signal to client");
                    serverTcs.SetResult();
                }, CancellationToken.None)
                .GetAwaiter();
        });

        this.Trace("connect");
        await ConnectAsync();

        this.Trace("listen detached");
        var listenTask = ListenAsync();

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

    [Fact]
    public async Task Listen_Normal()
    {
        this.Trace("start");

        // arrange
        this.Trace("generate messages");
        var messages = Enumerable.Range(0, 3)
            .Select(x => new string((char)x, 10))
            .ToArray();

        this.Trace("run server");
        await using var _ = RunServer(async (serverSocket, ct) =>
        {
            this.Trace("start sending messages");

            foreach (var message in messages)
            {
                await serverSocket.SendTextAsync(Encoding.UTF8.GetBytes(message), ct);
                await Task.Delay(1, CancellationToken.None);
            }

            this.Trace("done sending messages");
        });

        // act
        this.Trace("connect");
        await ConnectAsync();

        this.Trace("listen detached");
        ListenAsync().GetAwaiter();

        // assert
        this.Trace("assert text messages arrive");
        await Expect.To(() => _texts.IsEqual(messages), 1000);

        this.Trace("done");
    }

    [Fact]
    public async Task Listen_SmallBuffer()
    {
        this.Trace("start");

        // arrange
        this.Trace("generate messages");
        var messages = Enumerable.Range(0, 3)
            .Select(x => new string((char)x, 1_000_000))
            .ToArray();

        this.Trace("run server");
        await using var _ = RunServer(async (serverSocket, ct) =>
        {
            this.Trace("start sending messages");

            foreach (var message in messages)
            {
                await serverSocket.SendTextAsync(Encoding.UTF8.GetBytes(message), ct);
                await Task.Delay(1, CancellationToken.None);
            }

            this.Trace("done sending messages");
        });

        // act
        this.Trace("connect");
        await ConnectAsync();

        this.Trace("listen detached");
        var listenTask = ListenAsync();

        // assert
        this.Trace("assert text messages arrive");
        await Expect.To(() => _texts.IsEqual(messages), 1000);

        this.Trace("await listen task");
        var result = await listenTask;

        this.Trace("assert status is closed remote and no exception");
        result.Status.Is(WebSocketCloseStatus.ClosedRemote);
        result.Exception.IsDefault();

        this.Trace("done");
    }

    [Fact]
    public async Task Listen_BothTypes()
    {
        this.Trace("start");

        // arrange
        this.Trace("generate messages");
        var texts = Enumerable.Range(0, 3)
            .Select(x => new string((char)x, 10))
            .ToArray();
        var binaries = texts
            .Select(Encoding.UTF8.GetBytes)
            .ToArray();

        this.Trace("run server");
        await using var _ = RunServer(async (serverSocket, ct) =>
        {
            this.Trace("start sending messages");

            foreach (var message in texts)
            {
                await serverSocket.SendTextAsync(Encoding.UTF8.GetBytes(message), ct);
                await Task.Delay(1, CancellationToken.None);
            }

            foreach (var message in binaries)
            {
                await serverSocket.SendBinaryAsync(message, ct);
                await Task.Delay(1, CancellationToken.None);
            }

            this.Trace("done sending messages");
        });

        // act
        this.Trace("connect");
        await ConnectAsync();

        this.Trace("listen detached");
        var listenTask = ListenAsync();

        // assert
        this.Trace("assert text messages arrive");
        await Expect.To(() => _texts.IsEqual(texts), 1000);

        this.Trace("assert binary messages arrive");
        await Expect.To(() => _binaries.IsEqual(binaries), 1000);

        this.Trace("await listen task");
        var result = await listenTask;

        this.Trace("assert status is closed remote and no exception");
        result.Status.Is(WebSocketCloseStatus.ClosedRemote);
        result.Exception.IsDefault();

        this.Trace("done");
    }

    public async Task InitializeAsync()
    {
        this.Trace("start");

        _clientSocket = new NativeClientWebSocket();
        _managedSocket = new ManagedWebSocket(_clientSocket, Logger);
        this.Trace<string, string>("created pair of {clientSocket} and {managedSocket}", _clientSocket.GetFullId(), _managedSocket.GetFullId());

        _managedSocket.TextReceived += x =>
        {
            var message = Encoding.UTF8.GetString(x.Span);
            _texts.Enqueue(message);
        };
        _managedSocket.BinaryReceived += x =>
        {
            var message = x.ToArray();
            _binaries.Enqueue(message);
        };

        await Task.CompletedTask;

        this.Trace("done");
    }

    public async Task DisposeAsync()
    {
        await Task.CompletedTask;
    }

    private IAsyncDisposable RunServer(Func<ManagedWebSocket, CancellationToken, Task> handleWebSocket)
    {
        return RunServerBase(async (sp, ctx, ct) =>
        {
            this.Trace("start");

            var socket = new ManagedWebSocket(ctx.WebSocket, sp.Resolve<ILogger>());

            this.Trace<string>("handle {socket}", socket.GetFullId());
            await handleWebSocket(socket, ct);

            this.Trace("done");
        });
    }

    private async Task ConnectAndStartListenAsync(CancellationToken ct = default)
    {
        this.Trace("start");

        await ConnectAsync(ct);
        ListenAsync(ct).GetAwaiter();

        this.Trace("done");
    }

    private async Task ConnectAsync(CancellationToken ct = default)
    {
        this.Trace("start");

        await _clientSocket.ConnectAsync(ServerUri, ct);

        this.Trace("done");
    }

    private async Task<WebSocketCloseResult> ListenAsync(CancellationToken ct = default)
    {
        this.Trace("start");

        var result = await _managedSocket.ListenAsync(ct);

        this.Trace("done");

        return result;
    }

    private async Task<WebSocketSendStatus> SendTextAsync(string text, CancellationToken ct = default)
    {
        this.Trace("start");

        var result = await _managedSocket.SendTextAsync(Encoding.UTF8.GetBytes(text), ct);

        this.Trace("done");

        return result;
    }

    private async Task<WebSocketSendStatus> SendBinaryAsync(byte[] data, CancellationToken ct = default)
    {
        this.Trace("start");

        var result = await _managedSocket.SendBinaryAsync(data, ct);

        this.Trace("done");

        return result;
    }
}