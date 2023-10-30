using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Net.Sockets.Tests;

public abstract class ClientServerSocketTestsBase : TestBase, IAsyncLifetime
{
    protected const SocketMode SocketMode = Sockets.SocketMode.Messaging;
    private ClientSocket _clientSocket = default!;
    private readonly List<byte> _stream = new();

    protected ClientServerSocketTestsBase(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    protected async Task Send_NotConnected_Base()
    {
        this.Trace("start");

        // arrange
        var message = "demo"u8.ToArray();

        // act
        this.Trace("send text");
        var result = await SendAsync(message);

        // assert
        this.Trace("assert closed");
        result.Is(SocketSendStatus.Closed);

        this.Trace("done");
    }

    protected async Task Send_Canceled_Base()
    {
        this.Trace("start");

        // arrange
        var message = "demo"u8.ToArray();

        this.Trace("run server");
        await using var _ = RunServer(async (serverSocket, ct) => await serverSocket.WhenDisconnected(ct));

        this.Trace("connect");
        await ConnectAsync();

        // act
        this.Trace("send text");
        var result = await SendAsync(message, new CancellationToken(true));

        // assert
        this.Trace("assert canceled");
        result.Is(SocketSendStatus.Canceled);

        this.Trace("done");
    }

    protected async Task Send_ClientClosed_Base()
    {
        this.Trace("start");

        // arrange
        var message = "demo"u8.ToArray();
        var serverConnectionTcs = new TaskCompletionSource();

        this.Trace("run server");
        await using var _ = RunServer(
            async (serverSocket, ct) =>
            {
                this.Trace("send signal to client");
                var disconnectionTask = serverSocket.WhenDisconnected(ct);
                serverConnectionTcs.SetResult();
                await disconnectionTask;
            }
        );

        this.Trace("connect");
        await ConnectAsync();

        this.Trace("server connected");
        await serverConnectionTcs.Task;

        // act
        this.Trace("disconnect");
        await DisconnectAsync();

        this.Trace("send text");
        var result = await SendAsync(message);

        // assert
        this.Trace("assert closed");
        result.Is(SocketSendStatus.Closed);

        this.Trace("done");
    }

    protected async Task Send_ServerClosed_Base()
    {
        this.Trace("start");

        // arrange
        var message = "demo"u8.ToArray();

        this.Trace("run server");
        await using var _ = RunServer(
            (serverSocket, _) =>
            {
                this.Trace("disconnect server socket");
                serverSocket.Disconnect();

                return Task.CompletedTask;
            }
        );

        this.Trace("connect");
        var disconnectionTask = _clientSocket.WhenDisconnected();
        await ConnectAsync();

        this.Trace("await until disconnected");
        await disconnectionTask;

        // act
        this.Trace("send text");
        var result = await SendAsync(message);

        // assert
        this.Trace("assert closed");
        result.Is(SocketSendStatus.Closed);

        this.Trace("done");
    }

    protected async Task Send_Normal_Base()
    {
        this.Trace("start");

        // arrange
        var message = "demo"u8.ToArray();
        var serverConnectionTcs = new TaskCompletionSource();

        this.Trace("run server");
        await using var _ = RunServer(
            async (serverSocket, ct) =>
            {
                this.Trace("subscribe to messages");
                serverSocket.OnReceived += x =>
                    serverSocket.SendAsync(x.ToArray(), CancellationToken.None).GetAwaiter().GetResult();
                this.Trace("server subscribed to messages");

                this.Trace("send signal to client");
                var disconnectionTask = serverSocket.WhenDisconnected(ct);
                serverConnectionTcs.TrySetResult();
                await disconnectionTask;

                this.Trace("server socket closed");
            }
        );

        this.Trace("connect");
        await ConnectAsync();

        this.Trace("server connected");
        await serverConnectionTcs.Task;

        // act && assert
        this.Trace("send");
        var binaryResult = await SendAsync(message);

        this.Trace("assert ok");
        binaryResult.Is(SocketSendStatus.Ok);

        this.Trace("assert message arrived");
        await Expect.To(() => _stream.IsEqual(message));

        this.Trace("done");
    }

    protected async Task Send_Reconnect_Base()
    {
        this.Trace("start");

        // arrange
        var message = "demo"u8.ToArray();
        var serverConnectionTcs = new TaskCompletionSource();

        this.Trace("run server");
        await using var _ = RunServer(
            async (serverSocket, ct) =>
            {
                this.Trace("subscribe to messages");
                serverSocket.OnReceived += x =>
                    serverSocket.SendAsync(x.ToArray(), CancellationToken.None).GetAwaiter().GetResult();
                this.Trace("server subscribed to messages");

                this.Trace("send signal to client");
                var disconnectionTask = serverSocket.WhenDisconnected(ct);
                serverConnectionTcs.TrySetResult();
                await disconnectionTask;

                this.Trace("server socket closed");
            }
        );

        // act - send text
        this.Trace("connect");
        await ConnectAsync();

        this.Trace("server connected");
        await serverConnectionTcs.Task;

        this.Trace("send");
        var result = await SendAsync(message);

        this.Trace("assert sent ok");
        result.Is(SocketSendStatus.Ok);

        this.Trace("assert message arrived");
        await Expect.To(() => _stream.IsEqual(message));

        this.Trace("disconnect");
        await DisconnectAsync();

        // act - send text
        _stream.Clear();
        this.Trace("connect");
        serverConnectionTcs = new TaskCompletionSource();
        await ConnectAsync();

        this.Trace("server connected");
        await serverConnectionTcs.Task;

        this.Trace("send");
        result = await SendAsync(message);

        this.Trace("assert sent ok");
        result.Is(SocketSendStatus.Ok);

        this.Trace("assert message arrived");
        await Expect.To(() => _stream.IsEqual(message));

        this.Trace("disconnect");
        await DisconnectAsync();

        this.Trace("done");
    }

    protected async Task Listen_Normal_Base()
    {
        this.Trace("start");

        // arrange
        this.Trace("generate messages");
        var (message, chunks) = GenerateMessage(100, 10);
        var clientTcs = new TaskCompletionSource();

        this.Trace("run server");
        await using var _ = RunServer(
            async (serverSocket, _) =>
            {
                this.Trace("start sending chunks");

                var i = 0;
                foreach (var chunk in chunks)
                {
                    this.Trace("send chunk#{num}", ++i);
                    await serverSocket.SendAsync(chunk, CancellationToken.None);
                    await Task.Delay(1, CancellationToken.None);
                }

                this.Trace("sending chunks complete");

                await clientTcs.Task;
            }
        );

        // act
        this.Trace("connect");
        await ConnectAsync();

        // assert
        this.Trace("assert data arrived");
        await Expect.To(() => _stream.IsEqual(message), 1000);
        clientTcs.SetResult();

        this.Trace("done");
    }

    protected async Task Listen_SmallBuffer_Base()
    {
        this.Trace("start");

        // arrange
        var (message, chunks) = GenerateMessage(1_000_000, 100_000);
        var clientTcs = new TaskCompletionSource();

        this.Trace("run server");
        await using var _ = RunServer(
            async (serverSocket, _) =>
            {
                this.Trace("start sending chunks");

                var i = 0;
                foreach (var chunk in chunks)
                {
                    this.Trace("send chunk#{num}", ++i);
                    await serverSocket.SendAsync(chunk, CancellationToken.None);
                    await Task.Delay(1, CancellationToken.None);
                }

                this.Trace("sending chunks complete");

                await clientTcs.Task;
            }
        );

        // act
        this.Trace("connect");
        var disconnectionTask = _clientSocket.WhenDisconnected();
        await ConnectAsync();

        // assert
        this.Trace("assert data arrived");
        await Expect.To(() => _stream.Count.Is(message.Length));

        this.Trace("verify stream to be equal to message");
        _stream.SequenceEqual(message).IsTrue();

        this.Trace("disconnect");
        clientTcs.SetResult();
        var result = await disconnectionTask;

        this.Trace("assert closed remote");
        result.Is(SocketCloseStatus.ClosedRemote);

        this.Trace("done");
    }

    protected async Task Listen_Reconnect_Base()
    {
        this.Trace("start");

        // arrange
        this.Trace("generate messages");
        var (message, chunks) = GenerateMessage(1_000_000, 100_000);
        var clientTcs = new TaskCompletionSource();
        var connectionIndex = 0;
        var connectionsCount = 3;

        this.Trace("run server");
        await using var _ = RunServer(
            async (serverSocket, _) =>
            {
                this.Trace("start sending chunks");

                connectionIndex++;

                var complete = connectionIndex == connectionsCount;

                var i = 0;
                var breakAtChunk = complete ? int.MaxValue : new Random().Next(1, chunks.Count - 1);
                foreach (var chunk in chunks)
                {
                    i++;

                    // emulate disconnection
                    if (i == breakAtChunk)
                    {
                        this.Trace(
                            "disconnect, connection {connectionIndex}/{connectionsCount} at chunk#{num}",
                            connectionIndex,
                            connectionsCount,
                            i
                        );
                        return;
                    }

                    this.Trace("send chunk#{num}", i);
                    await serverSocket.SendAsync(chunk, CancellationToken.None);

                    await Task.Delay(1, CancellationToken.None);
                }

                this.Trace("sending chunks complete");

                // await until 3-rd connection is handled
                this.Trace("wait for signal from client");
                await clientTcs.Task;
            }
        );

        this.Trace("set disconnect handler");
        _clientSocket.OnDisconnected += _ =>
        {
            this.Trace("disconnected, clear stream");
            _stream.Clear();
        };

        this.Trace("connect");
        await ConnectAsync();

        // assert
        this.Trace("assert data arrived");
        await Expect.To(() => _stream.Count.Is(message.Length));

        this.Trace("verify stream to be equal to message");
        _stream.SequenceEqual(message).IsTrue();

        this.Trace("send signal to stop server");
        clientTcs.SetResult();

        this.Trace("disconnect");
        await DisconnectAsync();

        this.Trace("done");
    }

    public Task InitializeAsync()
    {
        this.Trace("start");

        var options = ClientSocketOptions.Default with { Mode = SocketMode, ReconnectDelay = 1 };
        _clientSocket = new ClientSocket(options, Logger);
        _clientSocket.OnReceived += x => _stream.AddRange(x.ToArray());

        _clientSocket.OnConnected += () => this.Trace("STATE: Connected");
        _clientSocket.OnDisconnected += status => this.Trace("STATE: Disconnected: {status}", status);
        _clientSocket.OnError += e => Assert.Fail($"Exception occured: {e}");

        this.Trace("done");

        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        this.Trace("start");

        _clientSocket.Disconnect();

        this.Trace("done");

        return Task.CompletedTask;
    }

    protected abstract void HandleConnect(IClientSocket socket);
    protected abstract IAsyncDisposable RunServer(Func<IServerSocket, CancellationToken, Task> handleSocket);

    private async Task ConnectAsync()
    {
        this.Trace("start");

        var tcs = new TaskCompletionSource();

        _clientSocket.Trace<string>("subscribe {tcs} to OnConnected", tcs.GetFullId());

        void HandleConnected()
        {
            _clientSocket.Trace<string>("set {tcs} to signaled state", tcs.GetFullId());
            tcs.SetResult();
            _clientSocket.OnConnected -= HandleConnected;
        }

        _clientSocket.OnConnected += HandleConnected;

        HandleConnect(_clientSocket);

        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(1));

        this.Trace("done");
    }

    private async Task DisconnectAsync()
    {
        this.Trace("start");

        var tcs = new TaskCompletionSource();

        _clientSocket.Trace<string>("subscribe {tcs} to OnConnected", tcs.GetFullId());

        void HandleDisconnected(SocketCloseStatus status)
        {
            _clientSocket.Trace("set {tcs} to signaled state with status {status}", tcs.GetFullId(), status);
            tcs.TrySetResult();
            _clientSocket.OnDisconnected -= HandleDisconnected;
        }

        _clientSocket.OnDisconnected += HandleDisconnected;

        _clientSocket.Disconnect();

        await tcs.Task;

        this.Trace("done");
    }

    private async Task<SocketSendStatus> SendAsync(byte[] data, CancellationToken ct = default)
    {
        this.Trace("start");

        var result = await _clientSocket.SendAsync(data, ct);

        this.Trace("done");

        return result;
    }
}
