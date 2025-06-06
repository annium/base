using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.Servers.Web;
using Xunit;

namespace Annium.Net.WebSockets.Tests;

public abstract class TestBase : Testing.TestBase
{
    private static int _basePort = 35000;
    protected readonly Uri ServerUri;
    private readonly int _port;

    protected TestBase(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        _port = Interlocked.Increment(ref _basePort);
        ServerUri = new Uri($"ws://127.0.0.1:{_port}");
    }

    protected IAsyncDisposable RunServerBase(
        Func<IServiceProvider, HttpListenerWebSocketContext, CancellationToken, Task> handleWebSocket
    )
    {
        var sp = Get<IServiceProvider>();
        var server = ServerBuilder
            .New(sp, _port)
            .WithWebSocketHandler(new WebSocketHandler(sp, handleWebSocket))
            .Build();
        var cts = new CancellationTokenSource();
        var serverTask = server.RunAsync(cts.Token);

        return Disposable.Create(async () =>
        {
            this.Trace("start");

            // await before cancellation for a while
            await Task.Delay(5, CancellationToken.None);

            this.Trace("cancel server run");
            await cts.CancelAsync();

            this.Trace("await server task");
#pragma warning disable VSTHRD003
            await serverTask;
#pragma warning restore VSTHRD003

            this.Trace("done");
        });
    }
}

file class WebSocketHandler : IWebSocketHandler
{
    private readonly IServiceProvider _sp;
    private readonly Func<IServiceProvider, HttpListenerWebSocketContext, CancellationToken, Task> _handle;

    public WebSocketHandler(
        IServiceProvider sp,
        Func<IServiceProvider, HttpListenerWebSocketContext, CancellationToken, Task> handle
    )
    {
        _sp = sp;
        _handle = handle;
    }

    public Task HandleAsync(HttpListenerWebSocketContext ctx, CancellationToken ct)
    {
        return _handle(_sp, ctx, ct);
    }
}
