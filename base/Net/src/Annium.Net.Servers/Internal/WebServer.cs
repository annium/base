using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Extensions.Execution;
using Annium.Logging;

namespace Annium.Net.Servers.Internal;

internal class WebServer : IWebServer, ILogSubject
{
    public ILogger Logger { get; }
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
        Logger = sp.Resolve<ILogger>();
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
        this.Trace("start");

        if (_listener.IsListening)
            throw new InvalidOperationException("Server is already started");

        this.Trace("start executor");
        _executor.Start(ct);

        this.Trace("start listener");
        _listener.Start();

        while (!ct.IsCancellationRequested)
        {
            HttpListenerContext listenerContext;
            try
            {
                // await for connection
                listenerContext = await _listener.GetContextAsync().WaitAsync(ct);
                this.Trace("socket accepted");
            }
            catch (OperationCanceledException)
            {
                this.Trace("break, operation canceled");
                break;
            }

            // try schedule socket handling
            if (_executor.TrySchedule(HandleRequest(_sp, listenerContext, ct)))
            {
                this.Trace("socket handle scheduled");
                continue;
            }

            this.Trace("closed and dispose socket (server is already stopping)");
            await CloseConnection(_sp, listenerContext, ct);
        }

        // when cancelled - await connections processing and stop listener
        this.Trace("dispose executor");
        await _executor.DisposeAsync().ConfigureAwait(false);

        this.Trace("stop listener");
        _listener.Stop();
    }

    private Func<ValueTask> HandleRequest(IServiceProvider sp, HttpListenerContext ctx, CancellationToken ct) => async () =>
    {
        if (ctx.Request.IsWebSocketRequest)
            await _handleWebSocketRequest(sp, ctx, ct);
        else
            await _handleHttpRequest(sp, ctx, ct);
    };

    private async Task HandleWebSocketRequest(IServiceProvider sp, HttpListenerContext ctx, CancellationToken ct)
    {
        this.Trace("start");

        var statusCode = 200;
        var isAborted = false;
        try
        {
            this.Trace("accept web socket");
            var webSocketContext = await ctx.AcceptWebSocketAsync(subProtocol: null);

            this.Trace("handle web socket");
            await _handleWebSocket(sp, webSocketContext, ct).ConfigureAwait(false);

            this.Trace("detect web socket state");
            isAborted = webSocketContext.WebSocket.State is WebSocketState.Aborted;
        }
        catch (OperationCanceledException e)
        {
            this.Trace("handle canceled: {e}", e);
        }
        catch (Exception e)
        {
            this.Trace("handle failed: {e}", e);
            statusCode = 500;
        }
        finally
        {
            if (isAborted)
            {
                this.Trace("web socket was aborted - abort response");
                ctx.Response.Abort();
            }
            else
            {
                this.Trace("web socket was not aborted - close normally");
                ctx.Response.StatusCode = statusCode;
                ctx.Response.Close();
            }
        }

        this.Trace("done");
    }

    private Task CloseConnection(IServiceProvider sp, HttpListenerContext ctx, CancellationToken ct)
    {
        this.Trace("start");

        ctx.Response.StatusCode = 404;
        ctx.Response.Close();

        this.Trace("done");

        return Task.CompletedTask;
    }

    private Task IgnoreWebSocket(IServiceProvider sp, HttpListenerWebSocketContext ctx, CancellationToken ct)
    {
        this.Trace("done");

        return Task.CompletedTask;
    }
}