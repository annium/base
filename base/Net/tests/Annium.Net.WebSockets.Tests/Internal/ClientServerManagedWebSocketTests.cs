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

namespace Annium.Net.WebSockets.Tests.Internal;

public class ClientServerManagedWebSocketTests : TestBase, IAsyncLifetime
{
    private IClientManagedWebSocket _clientSocket = default!;
    private readonly ConcurrentQueue<string> _texts = new();
    private readonly ConcurrentQueue<byte[]> _binaries = new();

    public ClientServerManagedWebSocketTests(ITestOutputHelper outputHelper) : base(outputHelper)
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
        await using var _ = RunServer(async serverSocket => await serverSocket.IsClosed);

        this.Trace("connect");
        await ConnectAsync();

        // act
        this.Trace("send message");
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

        this.Trace("run server");
        await using var _ = RunServer(async serverSocket => await serverSocket.IsClosed);

        this.Trace("connect");
        await ConnectAsync();

        // act
        this.Trace("disconnect client socket");
        _clientSocket.DisconnectAsync().GetAwaiter();

        this.Trace("send message");
        var result = await SendTextAsync(message);

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
        var serverTcs = new TaskCompletionSource();

        this.Trace("run server");
        await using var _ = RunServer(async serverSocket =>
        {
            this.Trace("disconnect server socket");
            await serverSocket.DisconnectAsync();

            Task.Delay(10, CancellationToken.None).ContinueWith(_ =>
            {
                this.Trace("send signal to client");
                serverTcs.SetResult();
            }, CancellationToken.None).GetAwaiter();
        });

        this.Trace("connect");
        await ConnectAsync();

        // delay to let server close connection
        this.Trace("await server signal");
        await serverTcs.Task;

        // act
        this.Trace("send message");
        var result = await SendTextAsync(message);

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
        const string text = "demo";
        var binary = Encoding.UTF8.GetBytes(text);
        var serverTcs = new TaskCompletionSource();

        this.Trace("run server");
        await using var _ = RunServer(async serverSocket =>
        {
            this.Trace("subscribe to text messages");
            serverSocket.TextReceived += x => serverSocket
                .SendTextAsync(x.ToArray(), CancellationToken.None)
                .GetAwaiter()
                .GetResult();
            this.Trace("server subscribed to text");

            this.Trace("subscribe to binary messages");
            serverSocket.BinaryReceived += x => serverSocket
                .SendBinaryAsync(x.ToArray(), CancellationToken.None)
                .GetAwaiter()
                .GetResult();
            this.Trace("server subscribed to binary");

            Task.Delay(10, CancellationToken.None).ContinueWith(_ =>
            {
                this.Trace("send signal to client");
                serverTcs.SetResult();
            }, CancellationToken.None).GetAwaiter();

            this.Trace("listen server socket");
            await serverSocket.IsClosed;

            this.Trace("server socket closed");
        });

        this.Trace("connect");
        await ConnectAsync();

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
    public async Task Send_Reconnect()
    {
        this.Trace("start");

        // arrange
        const string text = "demo";
        var binary = Encoding.UTF8.GetBytes(text);
        var serverConnectionTcs = new TaskCompletionSource();

        this.Trace("run server");
        await using var _ = RunServer(async serverSocket =>
        {
            this.Trace("subscribe to text messages");
            serverSocket.TextReceived += x => serverSocket
                .SendTextAsync(x.ToArray(), CancellationToken.None)
                .GetAwaiter()
                .GetResult();
            this.Trace("server subscribed to text");

            this.Trace("subscribe to binary messages");
            serverSocket.BinaryReceived += x => serverSocket
                .SendBinaryAsync(x.ToArray(), CancellationToken.None)
                .GetAwaiter()
                .GetResult();
            this.Trace("server subscribed to binary");

            this.Trace("send signal to client");
            serverConnectionTcs.TrySetResult();

            this.Trace("await server socket closed");
            await serverSocket.IsClosed;

            this.Trace("server socket closed");
        });

        // act - send text
        this.Trace("connect");
        await ConnectAsync();

        this.Trace("await server signal");
        await serverConnectionTcs.Task;

        this.Trace("send text");
        var textResult = await SendTextAsync(text);
        textResult.Is(WebSocketSendStatus.Ok);

        this.Trace("assert text message arrive");
        var expectedTexts = new[] { text };
        await Expect.To(() => _texts.IsEqual(expectedTexts));

        this.Trace("disconnect");
        await _clientSocket.DisconnectAsync();

        // act - send binary
        this.Trace("connect");
        serverConnectionTcs = new TaskCompletionSource();
        await ConnectAsync();

        this.Trace("await server signal");
        await serverConnectionTcs.Task;

        this.Trace("send binary");
        var binaryResult = await SendBinaryAsync(binary);
        binaryResult.Is(WebSocketSendStatus.Ok);

        this.Trace("assert binary message arrive");
        var expectedBinaries = new[] { binary };
        await Expect.To(() => _binaries.IsEqual(expectedBinaries));

        this.Trace("disconnect");
        await _clientSocket.DisconnectAsync();

        this.Trace("done");
    }

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

        this.Trace("cancel cts");
        cts.Cancel();

        // act
        this.Trace("await closed state");
        var result = await _clientSocket.IsClosed;

        // assert
        this.Trace("assert closed local and no exception");
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
        await using var _ = RunServer(async serverSocket => await serverSocket.IsClosed);

        this.Trace("connect");
        await ConnectAsync();

        this.Trace("disconnect client socket");
        await _clientSocket.DisconnectAsync();

        // act
        this.Trace("await closed state");
        var result = await _clientSocket.IsClosed;

        // assert
        this.Trace("assert closed local and no exception");
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
        await using var _ = RunServer(async serverSocket => await serverSocket.DisconnectAsync());

        this.Trace("connect");
        await ConnectAsync();

        // act
        this.Trace("await closed state");
        var result = await _clientSocket.IsClosed;

        // assert
        this.Trace("assert closed remote and no exception");
        result.Status.Is(WebSocketCloseStatus.ClosedRemote);
        result.Exception.IsDefault();

        this.Trace("done");
    }

    [Fact]
    public async Task Listen_Normal()
    {
        this.Trace("start");

        // arrange
        var messages = Enumerable.Range(0, 3)
            .Select(x => $"msg {x}")
            .ToArray();

        this.Trace("run server");
        await using var _ = RunServer(async serverSocket =>
        {
            this.Trace("start sending messages");

            foreach (var message in messages)
            {
                await serverSocket.SendTextAsync(Encoding.UTF8.GetBytes(message));
                await Task.Delay(1, CancellationToken.None);
            }

            this.Trace("done sending messages");
        });

        // act
        this.Trace("connect");
        await ConnectAsync();

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
        await using var _ = RunServer(async serverSocket =>
        {
            this.Trace("start sending messages");

            foreach (var message in messages)
            {
                await serverSocket.SendTextAsync(Encoding.UTF8.GetBytes(message));
                await Task.Delay(1, CancellationToken.None);
            }

            this.Trace("done sending messages");
        });

        // act
        this.Trace("connect");
        await ConnectAsync();

        // assert
        this.Trace("assert text messages arrive");
        await Expect.To(() => _texts.IsEqual(messages), 1000);

        this.Trace("await closed state");
        var result = await _clientSocket.IsClosed;

        this.Trace("assert closed remote and no exception");
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
        await using var _ = RunServer(async serverSocket =>
        {
            this.Trace("start sending messages");

            foreach (var message in texts)
            {
                await serverSocket.SendTextAsync(Encoding.UTF8.GetBytes(message));
                await Task.Delay(1, CancellationToken.None);
            }

            foreach (var message in binaries)
            {
                await serverSocket.SendBinaryAsync(message);
                await Task.Delay(1, CancellationToken.None);
            }

            this.Trace("done sending messages");
        });

        // act
        await ConnectAsync();

        // assert
        this.Trace("assert text messages arrive");
        await Expect.To(() => _texts.IsEqual(texts), 1000);

        this.Trace("assert binary messages arrive");
        await Expect.To(() => _binaries.IsEqual(binaries), 1000);

        this.Trace("await closed state");
        var result = await _clientSocket.IsClosed;

        this.Trace("assert closed remote and no exception");
        result.Status.Is(WebSocketCloseStatus.ClosedRemote);
        result.Exception.IsDefault();

        this.Trace("done");
    }

    public async Task InitializeAsync()
    {
        this.Trace("start");

        _clientSocket = new ClientManagedWebSocket(Logger);
        _clientSocket.TextReceived += x =>
        {
            var message = Encoding.UTF8.GetString(x.Span);
            _texts.Enqueue(message);
        };
        _clientSocket.BinaryReceived += x =>
        {
            var message = x.ToArray();
            _binaries.Enqueue(message);
        };

        await Task.CompletedTask;

        this.Trace("done");
    }

    public async Task DisposeAsync()
    {
        this.Trace("start");

        await _clientSocket.DisconnectAsync();

        this.Trace("done");
    }

    private IAsyncDisposable RunServer(Func<IServerManagedWebSocket, Task> handleWebSocket)
    {
        return RunServerBase(async (sp, ctx, ct) =>
        {
            this.Trace("start");

            var socket = new ServerManagedWebSocket(ctx.WebSocket, sp.Resolve<ILogger>(), ct);

            this.Trace<string>("handle {socket}", socket.GetFullId());
            await handleWebSocket(socket);

            this.Trace<string>("disconnect {socket}", socket.GetFullId());
            await socket.DisconnectAsync();

            this.Trace("done");
        });
    }

    private async Task ConnectAsync(CancellationToken ct = default)
    {
        this.Trace("start");

        await _clientSocket.ConnectAsync(ServerUri, ct);

        this.Trace("done");
    }

    private async Task<WebSocketSendStatus> SendTextAsync(string text, CancellationToken ct = default)
    {
        this.Trace("start");

        var result = await _clientSocket.SendTextAsync(Encoding.UTF8.GetBytes(text), ct);

        this.Trace("done");

        return result;
    }

    private async Task<WebSocketSendStatus> SendBinaryAsync(byte[] data, CancellationToken ct = default)
    {
        this.Trace("start");

        var result = await _clientSocket.SendBinaryAsync(data, ct);

        this.Trace("done");

        return result;
    }
}