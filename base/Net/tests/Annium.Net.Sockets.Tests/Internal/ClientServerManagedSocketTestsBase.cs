using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.Sockets.Internal;
using Annium.Testing;
using Annium.Testing.Assertions;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Net.Sockets.Tests.Internal;

public abstract class ClientServerManagedSocketTestsBase : TestBase, IAsyncLifetime
{
    private IClientManagedSocket _clientSocket = default!;
    private readonly List<byte> _stream = new();

    protected ClientServerManagedSocketTestsBase(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    protected async Task Send_NotConnected_Base()
    {
        this.Trace("start");

        // arrange
        var message = "demo"u8.ToArray();

        // act
        this.Trace("send message");
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
        await using var _ = RunServer(async (serverSocket, _) => await serverSocket.IsClosed);

        this.Trace("connect");
        await ConnectAsync();

        // act
        this.Trace("send message");
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

        this.Trace("run server");
        await using var _ = RunServer(async (serverSocket, _) => await serverSocket.IsClosed);

        this.Trace("connect");
        await ConnectAsync();

        // act
        this.Trace("disconnect client socket");
        _clientSocket.DisconnectAsync().GetAwaiter();

        this.Trace("send message");
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
        var serverTcs = new TaskCompletionSource();

        this.Trace("run server");
        await using var _ = RunServer(async (serverSocket, _) =>
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
        var serverTcs = new TaskCompletionSource();

        this.Trace("run server");
        await using var _ = RunServer(async (serverSocket, _) =>
        {
            this.Trace("subscribe to messages");
            serverSocket.OnReceived += x => serverSocket
                .SendAsync(x.ToArray(), CancellationToken.None)
                .GetAwaiter()
                .GetResult();
            this.Trace("server subscribed to messages");

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
        this.Trace("send message");
        var textResult = await SendAsync(message);

        // assert
        this.Trace("assert send result is ok");
        textResult.Is(SocketSendStatus.Ok);

        this.Trace("assert messages arrive");
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
        await using var _ = RunServer(async (serverSocket, _) =>
        {
            this.Trace("subscribe to messages");
            serverSocket.OnReceived += x => serverSocket
                .SendAsync(x.ToArray(), CancellationToken.None)
                .GetAwaiter()
                .GetResult();
            this.Trace("server subscribed to messages");

            this.Trace("send signal to client");
            serverConnectionTcs.TrySetResult();

            this.Trace("await server socket closed");
            await serverSocket.IsClosed;

            this.Trace("server socket closed");
        });

        // act - send
        this.Trace("connect");
        await ConnectAsync();

        this.Trace("await server signal");
        await serverConnectionTcs.Task;

        this.Trace("send");
        var result = await SendAsync(message);
        result.Is(SocketSendStatus.Ok);

        this.Trace("assert message arrived");
        await Expect.To(() => _stream.IsEqual(message));

        this.Trace("disconnect");
        await _clientSocket.DisconnectAsync();

        // act - send
        _stream.Clear();
        this.Trace("connect");
        serverConnectionTcs = new TaskCompletionSource();
        await ConnectAsync();

        this.Trace("await server signal");
        await serverConnectionTcs.Task;

        this.Trace("send");
        result = await SendAsync(message);
        result.Is(SocketSendStatus.Ok);

        this.Trace("assert message arrived");
        await Expect.To(() => _stream.IsEqual(message));

        this.Trace("disconnect");
        await _clientSocket.DisconnectAsync();

        this.Trace("done");
    }

    protected async Task Listen_Canceled_Base()
    {
        this.Trace("start");

        // arrange
        this.Trace("run server");
        await using var _ = RunServer(async (serverSocket, _) => await serverSocket.IsClosed);

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
        result.Status.Is(SocketCloseStatus.ClosedLocal);
        result.Exception.IsDefault();

        this.Trace("done");
    }

    protected async Task Listen_ClientClosed_Base()
    {
        this.Trace("start");

        // arrange
        this.Trace("run server");
        await using var _ = RunServer(async (serverSocket, _) => await serverSocket.IsClosed);

        this.Trace("connect");
        await ConnectAsync();

        this.Trace("disconnect client socket");
        await _clientSocket.DisconnectAsync();

        // act
        this.Trace("await closed state");
        var result = await _clientSocket.IsClosed;

        // assert
        this.Trace("assert closed local and no exception");
        result.Status.Is(SocketCloseStatus.ClosedLocal);
        result.Exception.IsDefault();

        this.Trace("done");
    }

    protected async Task Listen_ServerClosed_Base()
    {
        this.Trace("start");

        // arrange
        this.Trace("run server");
        await using var _ = RunServer(async (serverSocket, _) => await serverSocket.DisconnectAsync());

        this.Trace("connect");
        await ConnectAsync();

        // act
        this.Trace("await closed state");
        var result = await _clientSocket.IsClosed;

        // assert
        this.Trace("assert closed remote and no exception");
        result.Status.Is(SocketCloseStatus.ClosedRemote);
        result.Exception.IsDefault();

        this.Trace("done");
    }

    protected async Task Listen_Normal_Base()
    {
        this.Trace("start");

        // arrange
        this.Trace("generate messages");
        var (message, chunks) = GenerateMessage(100, 10);

        this.Trace("run server");
        var clientTcs = new TaskCompletionSource();
        await using var _ = RunServer(async (serverSocket, _) =>
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

            this.Trace("await client signal");
            await clientTcs.Task;
        });

        // act
        this.Trace("connect");
        await ConnectAsync();

        // assert
        this.Trace("assert data arrived");
        await Expect.To(() => _stream.IsEqual(message), 1000);

        this.Trace("send signal to server");
        clientTcs.SetResult();

        this.Trace("done");
    }

    protected async Task Listen_SmallBuffer_Base()
    {
        this.Trace("start");

        // arrange
        this.Trace("generate messages");
        var (message, chunks) = GenerateMessage(1_000_000, 100_000);

        this.Trace("run server");
        var clientTcs = new TaskCompletionSource();
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

            this.Trace("await client signal");
            await clientTcs.Task;
        });

        // act
        this.Trace("connect");
        await ConnectAsync();

        // assert
        this.Trace("assert data arrived");
        await Expect.To(() => _stream.Count.Is(message.Length));

        this.Trace("verify stream to be equal to message");
        _stream.SequenceEqual(message).IsTrue();

        this.Trace("send signal to server");
        clientTcs.SetResult();

        this.Trace("await closed state");
        var result = await _clientSocket.IsClosed;

        this.Trace("assert closed remote and no exception");
        result.Status.Is(SocketCloseStatus.ClosedRemote);
        result.Exception.IsDefault();

        this.Trace("done");
    }

    public async Task InitializeAsync()
    {
        this.Trace("start");

        _clientSocket = new ClientManagedSocket(SocketMode.Raw, Logger);
        _clientSocket.OnReceived += x => _stream.AddRange(x.ToArray());

        await Task.CompletedTask;

        this.Trace("done");
    }

    public async Task DisposeAsync()
    {
        this.Trace("start");

        await _clientSocket.DisconnectAsync();

        this.Trace("done");
    }

    internal abstract Task HandleConnectAsync(IClientManagedSocket socket, CancellationToken ct);
    internal abstract IAsyncDisposable RunServer(Func<IServerManagedSocket, CancellationToken, Task> handleSocket);

    private async Task ConnectAsync(CancellationToken ct = default)
    {
        this.Trace("start");

        await HandleConnectAsync(_clientSocket, ct);

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