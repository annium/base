using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.Sockets.Internal;
using Annium.Testing;
using Annium.Testing.Assertions;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Net.Sockets.Tests.Internal;

public abstract class ManagedSocketTestsBase : TestBase, IAsyncLifetime
{
    private Socket _clientSocket = default!;
    private Stream _clientStream = default!;
    private ManagedSocket _managedSocket = default!;
    private readonly List<byte> _stream = new();

    public ManagedSocketTestsBase(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task Send_Canceled_Base()
    {
        this.Trace("start");

        // arrange
        var message = "demo"u8.ToArray();

        this.Trace("run server");
        await using var _ = RunServer(async (serverSocket, ct) => await serverSocket.ListenAsync(ct));

        this.Trace("connect and start listening");
        await ConnectAndStartListenAsync();

        // act
        this.Trace("send message with canceled flag");
        var result = await _managedSocket.SendAsync(message, new CancellationToken(true));

        // assert
        this.Trace("assert canceled is received");
        result.Is(SocketSendStatus.Canceled);

        this.Trace("done");
    }

    [Fact]
    public async Task Send_ClientClosed_Base()
    {
        this.Trace("start");

        // arrange
        var message = "demo"u8.ToArray();

        this.Trace("run server");
        await using var _ = RunServer(async (serverSocket, ct) => await serverSocket.ListenAsync(ct));

        this.Trace("connect and start listening");
        await ConnectAndStartListenAsync();

        // act
        this.Trace("close client socket");
        _clientSocket.Close();

        this.Trace("send message");
        var result = await _managedSocket.SendAsync(message);

        // assert
        this.Trace("assert close is received");
        result.Is(SocketSendStatus.Closed);

        this.Trace("done");
    }

    [Fact]
    public async Task Send_ServerClosed_Base()
    {
        // arrange
        this.Trace("start");

        var message = "demo"u8.ToArray();
        var serverTcs = new TaskCompletionSource();

        this.Trace("run server");
        await using var _ = RunServerBase(async (_, socket, _) =>
        {
            socket.LingerState = new LingerOption(true, 0);
            socket.Close();
            await Task.Delay(10, CancellationToken.None);
            serverTcs.SetResult();
        });

        this.Trace("connect and start listening");
        await ConnectAndStartListenAsync();

        // delay to let server close connection
        this.Trace("await server signal");
        await serverTcs.Task;

        // act
        this.Trace("send message");
        var result = await _managedSocket.SendAsync(message);

        // assert
        this.Trace("assert close is received");
        result.Is(SocketSendStatus.Closed);

        this.Trace("done");
    }

    [Fact]
    public async Task Send_Normal_Base()
    {
        // arrange
        this.Trace("start");

        var message = "demo"u8.ToArray();
        var serverTcs = new TaskCompletionSource();

        this.Trace("run server");
        await using var _ = RunServer(async (serverSocket, ct) =>
        {
            serverSocket.OnReceived += x => serverSocket
                .SendAsync(x.ToArray(), CancellationToken.None)
                .GetAwaiter();

            Task.Delay(10, CancellationToken.None)
                .ContinueWith(_ => serverTcs.SetResult(), CancellationToken.None)
                .GetAwaiter();

            await serverSocket.ListenAsync(ct);
        });

        this.Trace("connect and start listening");
        await ConnectAndStartListenAsync();

        // delay to let server close connection
        this.Trace("await server signal");
        await serverTcs.Task;

        // act
        this.Trace("send message");
        var messageResult = await _managedSocket.SendAsync(message);

        // assert
        this.Trace("assert ok is received");
        messageResult.Is(SocketSendStatus.Ok);

        this.Trace("assert message is echoed back");
        await Expect.To(() => _stream.IsEqual(message));

        this.Trace("done");
    }

    [Fact]
    public async Task Listen_Canceled_Base()
    {
        // arrange
        this.Trace("start");

        this.Trace("run server");
        await using var _ = RunServer(async (serverSocket, ct) => await serverSocket.ListenAsync(ct));

        this.Trace("connect");
        await ConnectAsync();

        // act
        this.Trace("listen with canceled token");
        var result = await ListenAsync(new CancellationToken(true));

        // assert
        this.Trace("assert closed local with no exception");
        result.Status.Is(SocketCloseStatus.ClosedLocal);
        result.Exception.IsDefault();

        this.Trace("done");
    }

    [Fact]
    public async Task Listen_ClientClosed_Base()
    {
        // arrange
        this.Trace("start");

        this.Trace("run server");
        await using var _ = RunServer(async (serverSocket, ct) => await serverSocket.ListenAsync(ct));

        this.Trace("connect");
        await ConnectAsync();

        this.Trace("close client socket");
        _clientSocket.Close();

        // act
        this.Trace("listen");
        var result = await ListenAsync();

        // assert
        this.Trace("assert closed local with no exception");
        result.Status.Is(SocketCloseStatus.ClosedLocal);
        result.Exception.IsDefault();

        this.Trace("done");
    }

    [Fact]
    public async Task Listen_ServerClosed_Base()
    {
        // arrange
        this.Trace("start");

        this.Trace("run server");
        await using var _ = RunServerBase((_, socket, _) =>
        {
            socket.Close();

            return Task.CompletedTask;
        });

        this.Trace("connect");
        await ConnectAsync();

        // act
        this.Trace("listen");
        var result = await ListenAsync();

        // assert
        this.Trace("assert closed remote with no exception");
        result.Status.Is(SocketCloseStatus.ClosedRemote);
        result.Exception.IsDefault();

        this.Trace("done");
    }

    [Fact]
    public async Task Listen_Normal_Base()
    {
        // arrange
        this.Trace("start");

        this.Trace("generate messages");
        var (message, chunks) = GenerateMessage(100, 10);

        this.Trace("run server");
        await using var _ = RunServer(async (serverSocket, ct) =>
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
        });

        // act
        this.Trace("connect");
        await ConnectAsync();

        this.Trace("listen detached");
        ListenAsync().GetAwaiter();

        // assert
        this.Trace("assert data arrived");
        await Expect.To(() => _stream.IsEqual(message), 1000);

        this.Trace("done");
    }

    [Fact]
    public async Task Listen_SmallBuffer_Base()
    {
        // arrange
        this.Trace("start");

        this.Trace("generate messages");
        var (message, chunks) = GenerateMessage(1_000_000, 100_000);

        this.Trace("run server");
        await using var _ = RunServer(async (serverSocket, ct) =>
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
        });

        // act
        this.Trace("connect");
        await ConnectAsync();

        this.Trace("listen detached");
        var listenTask = ListenAsync();

        // assert
        this.Trace("assert data arrived");
        await Expect.To(() => _stream.Count.Is(message.Length));

        this.Trace("verify stream to be equal to message");
        _stream.SequenceEqual(message).IsTrue();

        this.Trace("await listen completion");
        var result = await listenTask;

        this.Trace("assert closed remote with no exception");
        result.Status.Is(SocketCloseStatus.ClosedRemote);
        result.Exception.IsDefault();

        this.Trace("done");
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _clientStream.DisposeAsync();
    }

    internal abstract Stream WrapClientAsStream(Socket socket);
    internal abstract IAsyncDisposable RunServer(Func<ManagedSocket, CancellationToken, Task> handleSocket);

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

        _clientSocket = new Socket(SocketType.Stream, ProtocolType.Tcp)
        {
            NoDelay = true
        };

        this.Trace("connect");
        await _clientSocket.ConnectAsync(EndPoint, ct);
        _clientStream = new NetworkStream(_clientSocket);

        this.Trace("create managed socket");
        _managedSocket = new ManagedSocket(_clientStream, Logger);
        this.Trace<string, string>("created pair of {clientSocket} and {managedSocket}", _clientSocket.GetFullId(), _managedSocket.GetFullId());

        _managedSocket.OnReceived += x => _stream.AddRange(x.ToArray());

        await Task.CompletedTask;

        this.Trace("done");
    }

    private async Task<SocketCloseResult> ListenAsync(CancellationToken ct = default)
    {
        this.Trace("start");

        var result = await _managedSocket.ListenAsync(ct);

        this.Trace("done");

        return result;
    }
}