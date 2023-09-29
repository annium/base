using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Extensions.Execution;
using Annium.Logging;

namespace Annium.Net.Servers.Internal;

internal class WebServer : IWebServer
{
    private readonly IServiceProvider _sp;
    private readonly HttpListener _listener;
    private readonly Func<IServiceProvider, HttpListenerContext, CancellationToken, Task> _handleHttpRequest;
    private readonly Func<IServiceProvider, HttpListenerContext, CancellationToken, Task> _handleWebSocketRequest;
    private readonly Func<IServiceProvider, HttpListenerWebSocketContext, CancellationToken, Task> _handleWebSocket;
    private readonly IBackgroundExecutor _executor;

    public WebServer(
        IServiceProvider sp,
        int port,
        Func<IServiceProvider, HttpListenerContext, CancellationToken, Task>? handleHttp,
        Func<IServiceProvider, HttpListenerWebSocketContext, CancellationToken, Task>? handleWebSocket
    )
    {
        _sp = sp;
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://*:{port}/");
        _handleHttpRequest = handleHttp ?? CloseConnection;
        if (handleWebSocket is null)
        {
            _handleWebSocketRequest = CloseConnection;
            _handleWebSocket = IgnoreWebSocket;
        }
        else
        {
            _handleWebSocketRequest = HandleWebSocketRequest;
            _handleWebSocket = handleWebSocket;
        }

        _executor = Executor.Background.Parallel<WebServer>(_sp.Resolve<ILogger>());
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        if (_listener.IsListening)
            throw new InvalidOperationException("Server is already started");

        _executor.Start(ct);
        _listener.Start();

        while (!ct.IsCancellationRequested)
        {
            HttpListenerContext listenerContext;
            try
            {
                // await for connection
                listenerContext = await _listener.GetContextAsync().WaitAsync(ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            // schedule connection handling
            _executor.Schedule(async () => await HandleRequest(_sp, listenerContext, ct).ConfigureAwait(false));
        }

        // when cancelled - await connections processing and stop listener
        await _executor.DisposeAsync().ConfigureAwait(false);
        _listener.Stop();
    }

    private async Task HandleRequest(IServiceProvider sp, HttpListenerContext ctx, CancellationToken ct)
    {
        if (ctx.Request.IsWebSocketRequest)
            await _handleWebSocketRequest(sp, ctx, ct);
        else
            await _handleHttpRequest(sp, ctx, ct);
    }

    private async Task HandleWebSocketRequest(IServiceProvider sp, HttpListenerContext ctx, CancellationToken ct)
    {
        var statusCode = 200;
        var isAborted = false;
        try
        {
            var webSocketContext = await ctx.AcceptWebSocketAsync(subProtocol: null);
            await _handleWebSocket(sp, webSocketContext, ct).ConfigureAwait(false);
            isAborted = webSocketContext.WebSocket.State is WebSocketState.Aborted;
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception)
        {
            statusCode = 500;
        }
        finally
        {
            if (isAborted)
                ctx.Response.Abort();
            else
            {
                ctx.Response.StatusCode = statusCode;
                ctx.Response.Close();
            }
        }
    }

    private static Task CloseConnection(IServiceProvider sp, HttpListenerContext ctx, CancellationToken ct)
    {
        ctx.Response.StatusCode = 404;
        ctx.Response.Close();

        return Task.CompletedTask;
    }

    private static Task IgnoreWebSocket(IServiceProvider sp, HttpListenerWebSocketContext ctx, CancellationToken ct)
    {
        return Task.CompletedTask;
    }
}