using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Net.Sockets.Internal;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Net.Sockets.Tests.Internal;

public class ManagedSocketPlainTests : ManagedSocketTestsBase
{
    public ManagedSocketPlainTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
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
    public async Task Listen_Canceled()
    {
        this.Trace("start");

        await Listen_Canceled_Base();

        this.Trace("done");
    }

    [Fact]
    public async Task Listen_ClientClosed()
    {
        // arrange
        this.Trace("start");

        await Listen_ClientClosed_Base();

        this.Trace("done");
    }

    [Fact]
    public async Task Listen_ServerClosed()
    {
        this.Trace("start");

        await Listen_ServerClosed_Base();

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

    internal override Stream WrapClientAsStream(Socket socket)
    {
        return new NetworkStream(socket);
    }

    internal override IAsyncDisposable RunServer(Func<ManagedSocket, CancellationToken, Task> handleSocket)
    {
        return RunServerBase(async (sp, raw, ct) =>
        {
            this.Trace("start");

            this.Trace<string>("wrap {raw} into stream", raw.GetFullId());
            await using var stream = new NetworkStream(raw);
            var socket = new ManagedSocket(stream, sp.Resolve<ILogger>());

            this.Trace<string>("handle {socket}", socket.GetFullId());
            await handleSocket(socket, ct);

            this.Trace("done");
        });
    }
}