using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.Servers.Web;
using Xunit;

namespace Annium.Net.WebSockets.Tests;

/// <summary>
/// Base class for WebSocket tests providing common server setup functionality
/// </summary>
public abstract class TestBase : Testing.TestBase
{
    /// <summary>
    /// Base port number for test servers
    /// </summary>
    private static int _basePort = 35000;

    /// <summary>
    /// URI of the test server for this test instance
    /// </summary>
    protected readonly Uri ServerUri;

    /// <summary>
    /// Port number assigned to this test instance
    /// </summary>
    private readonly int _port;

    protected TestBase(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        _port = Interlocked.Increment(ref _basePort);
        ServerUri = new Uri($"ws://127.0.0.1:{_port}");
    }

    /// <summary>
    /// Runs a base WebSocket server for testing
    /// </summary>
    /// <param name="handleWebSocket">Function to handle WebSocket connections</param>
    /// <returns>Disposable representing the running server</returns>
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

/// <summary>
/// WebSocket handler implementation for test scenarios
/// </summary>
file class WebSocketHandler : IWebSocketHandler
{
    /// <summary>
    /// Service provider for dependency injection
    /// </summary>
    private readonly IServiceProvider _sp;

    /// <summary>
    /// Function to handle WebSocket connections
    /// </summary>
    private readonly Func<IServiceProvider, HttpListenerWebSocketContext, CancellationToken, Task> _handle;

    public WebSocketHandler(
        IServiceProvider sp,
        Func<IServiceProvider, HttpListenerWebSocketContext, CancellationToken, Task> handle
    )
    {
        _sp = sp;
        _handle = handle;
    }

    /// <summary>
    /// Handles incoming WebSocket connections
    /// </summary>
    /// <param name="ctx">WebSocket context</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Task representing the handling operation</returns>
    public Task HandleAsync(HttpListenerWebSocketContext ctx, CancellationToken ct)
    {
        return _handle(_sp, ctx, ct);
    }
}
