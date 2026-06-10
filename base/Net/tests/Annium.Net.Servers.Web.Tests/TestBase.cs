using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.Servers.Web;
using Xunit;

namespace Annium.Net.Servers.Web.Tests;

/// <summary>
/// Base class for Servers.Web tests, providing server factory helpers.
/// </summary>
public abstract class TestBase : Testing.TestBase
{
    /// <summary>
    /// Initializes a new instance of the TestBase class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    protected TestBase(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Starts a server with both HTTP and WebSocket handlers.
    /// </summary>
    /// <param name="httpHandle">HTTP request handler callback.</param>
    /// <param name="wsHandle">WebSocket connection handler callback.</param>
    /// <returns>The started server.</returns>
    protected IServer RunServer(
        Func<HttpListenerContext, CancellationToken, Task>? httpHandle = null,
        Func<HttpListenerWebSocketContext, CancellationToken, Task>? wsHandle = null
    )
    {
        var sp = Get<IServiceProvider>();
        var builder = ServerBuilder.New(sp);

        if (httpHandle is not null)
            builder = builder.WithHttpHandler(new DelegatingHttpHandler(httpHandle));

        if (wsHandle is not null)
            builder = builder.WithWebSocketHandler(new DelegatingWebSocketHandler(wsHandle));

        return builder.Start().NotNull();
    }

    /// <summary>
    /// Starts a server that only handles HTTP requests with a simple response body and 200 status.
    /// </summary>
    /// <param name="responseBody">Text to write to the response body.</param>
    /// <returns>The started server.</returns>
    protected IServer RunHttpServer(string responseBody = "ok")
    {
        return RunServer(
            httpHandle: async (ctx, _) =>
            {
                var data = System.Text.Encoding.UTF8.GetBytes(responseBody);
                ctx.Response.StatusCode = 200;
                await ctx.Response.OutputStream.WriteAsync(data);
                ctx.Response.Close();
            }
        );
    }
}

/// <summary>
/// HTTP handler that delegates to a provided callback.
/// </summary>
file class DelegatingHttpHandler : IHttpHandler
{
    /// <summary>The delegate that handles each incoming HTTP request.</summary>
    private readonly Func<HttpListenerContext, CancellationToken, Task> _handle;

    public DelegatingHttpHandler(Func<HttpListenerContext, CancellationToken, Task> handle)
    {
        _handle = handle;
    }

    /// <summary>Handles the incoming request by delegating to the configured callback.</summary>
    /// <param name="ctx">The HTTP listener context.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task representing the handling operation.</returns>
    public Task HandleAsync(HttpListenerContext ctx, CancellationToken ct) => _handle(ctx, ct);
}

/// <summary>
/// WebSocket handler that delegates to a provided callback.
/// </summary>
file class DelegatingWebSocketHandler : IWebSocketHandler
{
    /// <summary>The delegate that handles each incoming WebSocket request.</summary>
    private readonly Func<HttpListenerWebSocketContext, CancellationToken, Task> _handle;

    public DelegatingWebSocketHandler(Func<HttpListenerWebSocketContext, CancellationToken, Task> handle)
    {
        _handle = handle;
    }

    /// <summary>Handles the incoming request by delegating to the configured callback.</summary>
    /// <param name="ctx">The WebSocket connection context.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task representing the handling operation.</returns>
    public Task HandleAsync(HttpListenerWebSocketContext ctx, CancellationToken ct) => _handle(ctx, ct);
}
