using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Net.Servers;
using Xunit.Abstractions;

namespace Annium.Net.WebSockets.Tests;

public abstract class TestBase : Testing.Lib.TestBase
{
    private static int _basePort = 15000;
    protected readonly Uri ServerUri;
    private readonly int _port;

    protected TestBase(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _port = Interlocked.Increment(ref _basePort);
        ServerUri = new Uri($"ws://127.0.0.1:{_port}");
    }

    protected IAsyncDisposable RunServerBase(Func<IServiceProvider, HttpListenerWebSocketContext, CancellationToken, Task> handleWebSocket)
    {
        var server = WebServerBuilder.New(Get<IServiceProvider>(), _port).WithWebSockets(handleWebSocket).Build();
        var cts = new CancellationTokenSource();
        var serverTask = server.RunAsync(cts.Token);

        return Disposable.Create(async () =>
        {
            // await before cancellation for a while
            await Task.Delay(5, CancellationToken.None);
            cts.Cancel();
            await serverTask;
        });
    }
}