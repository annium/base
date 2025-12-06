using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Net.Servers.Web;
using Xunit;

namespace Annium.Net.WebSockets.Tests;

/// <summary>
/// Base class for WebSocket tests providing common server setup functionality
/// </summary>
public abstract class TestBase : Testing.TestBase
{
    protected TestBase(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Runs a base WebSocket server for testing
    /// </summary>
    /// <param name="handleWebSocket">Function to handle WebSocket connections</param>
    /// <returns>Disposable representing the running server</returns>
    protected IServer RunServerBase(
        Func<IServiceProvider, HttpListenerWebSocketContext, CancellationToken, Task> handleWebSocket
    )
    {
        var sp = Get<IServiceProvider>();
        var handler = new WebSocketHandler(sp, handleWebSocket);

        return ServerBuilder.New(sp).WithWebSocketHandler(handler).Start().NotNull();
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
