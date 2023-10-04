using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Net.Sockets.Tests;

public class ClientServerSocketPlainTests : ClientServerSocketTestsBase
{
    public ClientServerSocketPlainTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task Send_NotConnected()
    {
        this.Trace("start");

        await Send_NotConnected_Base();

        this.Trace("done");
    }

    [Fact]
    public async Task Send_Canceled()
    {
        this.Trace("start");

        await Send_Canceled_Base();

        this.Trace("done");
    }

    [Fact]
    public async Task Send_ClientClosed()
    {
        this.Trace("start");

        await Send_ClientClosed_Base();

        this.Trace("done");
    }

    [Fact]
    public async Task Send_ServerClosed()
    {
        this.Trace("start");

        await Send_ServerClosed_Base();

        this.Trace("done");
    }

    [Fact]
    public async Task Send_Normal()
    {
        this.Trace("start");

        await Send_Normal_Base();

        this.Trace("done");
    }

    [Fact]
    public async Task Send_Reconnect()
    {
        this.Trace("start");

        await Send_Reconnect_Base();

        this.Trace("done");
    }

    [Fact]
    public async Task Listen_Normal()
    {
        this.Trace("start");

        await Listen_Normal_Base();

        this.Trace("done");
    }

    [Fact]
    public async Task Listen_SmallBuffer()
    {
        this.Trace("start");

        await Listen_SmallBuffer_Base();

        this.Trace("done");
    }

    [Fact]
    public async Task Listen_Reconnect()
    {
        this.Trace("start");

        await Listen_Reconnect_Base();

        this.Trace("done");
    }

    protected override void HandleConnect(IClientSocket socket)
    {
        this.Trace("start");

        socket.Connect(EndPoint);

        this.Trace("done");
    }

    protected override IAsyncDisposable RunServer(Func<IServerSocket, CancellationToken, Task> handleSocket)
    {
        return RunServerBase(async (sp, raw, ct) =>
        {
            this.Trace("start");

            this.Trace<string>("wrap {raw} into network stream", raw.GetFullId());
            await using var stream = new NetworkStream(raw);

            this.Trace("create managed socket");
            var socket = new ServerSocket(stream, sp.Resolve<ILogger>(), ct);

            this.Trace<string>("handle {socket}", socket.GetFullId());
            await handleSocket(socket, ct);

            this.Trace<string>("disconnect {socket}", socket.GetFullId());
            socket.Disconnect();

            this.Trace("done");
        });
    }
}