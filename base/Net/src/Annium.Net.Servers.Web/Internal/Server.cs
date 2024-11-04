using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Execution.Background;
using Annium.Logging;

namespace Annium.Net.Servers.Web.Internal;

internal class Server : IServer, ILogSubject
{
    public ILogger Logger { get; }
    private readonly HttpListener _listener;
    private readonly Func<HttpListenerContext, CancellationToken, Task> _handleHttpRequest;
    private readonly Func<HttpListenerContext, CancellationToken, Task> _handleWebSocketRequest;
    private readonly Func<HttpListenerWebSocketContext, CancellationToken, Task> _handleWebSocket;
    private readonly IExecutor _executor;

    public Server(int port, IHttpHandler? httpHandler, IWebSocketHandler? webSocketHandler, ILogger logger)
    {
        Logger = logger;
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://*:{port}/");
        _handleHttpRequest = httpHandler is not null ? httpHandler.HandleAsync : CloseConnectionAsync;
        if (webSocketHandler is not null)
        {
            _handleWebSocketRequest = HandleWebSocketRequestAsync;
            _handleWebSocket = webSocketHandler.HandleAsync;
        }
        else
        {
            _handleWebSocketRequest = CloseConnectionAsync;
            _handleWebSocket = IgnoreWebSocketAsync;
        }

        _executor = Executor.Parallel<Server>(logger);
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
            if (_executor.Schedule(HandleRequest(listenerContext, ct)))
            {
                this.Trace("socket handle scheduled");
                continue;
            }

            this.Trace("closed and dispose socket (server is already stopping)");
            await CloseConnectionAsync(listenerContext, ct);
        }

        // when cancelled - await connections processing and stop listener
        this.Trace("dispose executor");
        await _executor.DisposeAsync().ConfigureAwait(false);

        this.Trace("stop listener");
        _listener.Stop();
    }

    private Func<ValueTask> HandleRequest(HttpListenerContext ctx, CancellationToken ct) =>
        async () =>
        {
            if (ctx.Request.IsWebSocketRequest)
            {
                this.Trace("handle websocket request");
                await _handleWebSocketRequest(ctx, ct);
            }
            else
            {
                this.Trace("handle http request");
                await _handleHttpRequest(ctx, ct);
            }
        };

    private async Task HandleWebSocketRequestAsync(HttpListenerContext ctx, CancellationToken ct)
    {
        this.Trace("start");

        var statusCode = 200;
        var isAborted = false;
        try
        {
            this.Trace("accept web socket");
            var webSocketContext = await ctx.AcceptWebSocketAsync(subProtocol: null);

            this.Trace("handle web socket");
            await _handleWebSocket(webSocketContext, ct).ConfigureAwait(false);

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

    private Task CloseConnectionAsync(HttpListenerContext ctx, CancellationToken ct)
    {
        this.Trace("start");

        ctx.Response.StatusCode = 404;
        ctx.Response.Close();

        this.Trace("done");

        return Task.CompletedTask;
    }

    private Task IgnoreWebSocketAsync(HttpListenerWebSocketContext ctx, CancellationToken ct)
    {
        this.Trace("done");

        return Task.CompletedTask;
    }
}
