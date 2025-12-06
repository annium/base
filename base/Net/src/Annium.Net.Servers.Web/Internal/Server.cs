using System;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Execution.Background;
using Annium.Linq;
using Annium.Logging;

namespace Annium.Net.Servers.Web.Internal;

/// <summary>
/// Internal implementation of a web server that handles HTTP and WebSocket requests.
/// </summary>
internal class Server : IServer, ILogSubject
{
    /// <summary>
    /// Gets the logger instance for this server.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Uri, that may be used to connect to server
    /// </summary>
    public Uri Uri { get; }

    /// <summary>
    /// Whether to use https:// or http:// scheme in listener.
    /// </summary>
    private readonly HttpListener _listener;

    /// <summary>
    /// The wrapper function used to handle HTTP requests.
    /// </summary>
    private readonly Func<HttpListenerContext, CancellationToken, Task> _handleHttpRequest;

    /// <summary>
    /// The function used to handle HTTP requests.
    /// </summary>
    private readonly Func<HttpListenerContext, CancellationToken, Task> _handleHttp;

    /// <summary>
    /// The wrapper function used to handle WebSocket requests.
    /// </summary>
    private readonly Func<HttpListenerContext, CancellationToken, Task> _handleWebSocketRequest;

    /// <summary>
    /// The function used to handle established WebSocket connections.
    /// </summary>
    private readonly Func<HttpListenerWebSocketContext, CancellationToken, Task> _handleWebSocket;

    /// <summary>
    /// Cancellation token source, that will trigger server to stop
    /// </summary>
    private readonly CancellationTokenSource _cts;

    /// <summary>
    /// Task, that will be completed, when server is stopped
    /// </summary>
    private readonly Task _whenStopped;

    /// <summary>
    /// Initializes a new instance of the Server class.
    /// </summary>
    /// <param name="listener">HttpListener, server will be working with</param>
    /// <param name="httpHandler">The HTTP handler to process HTTP requests, or null to return 404 for all HTTP requests.</param>
    /// <param name="webSocketHandler">The WebSocket handler to process WebSocket connections, or null to reject WebSocket upgrades.</param>
    /// <param name="uri">Uri, that will be exposed as base connection address of server</param>
    /// <param name="logger">The logger instance for this server.</param>
    public Server(
        HttpListener listener,
        IHttpHandler? httpHandler,
        IWebSocketHandler? webSocketHandler,
        Uri uri,
        ILogger logger
    )
    {
        Logger = logger;
        Uri = uri;

        _listener = listener;
        _cts = new CancellationTokenSource();

        if (httpHandler is not null)
        {
            _handleHttpRequest = HandleHttpRequestAsync;
            _handleHttp = httpHandler.HandleAsync;
        }
        else
        {
            _handleHttpRequest = CloseConnectionAsync;
            _handleHttp = IgnoreHttpAsync;
        }

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

        _whenStopped = Task.Factory.StartNew(RunAsync, TaskCreationOptions.LongRunning);
    }

    /// <summary>
    /// Stops the server by canceling execution and awaiting shutdown completion.
    /// </summary>
    /// <returns>A task that completes when the server has finished disposing.</returns>
    public async ValueTask DisposeAsync()
    {
        await _cts.CancelAsync();
#pragma warning disable VSTHRD003
        await _whenStopped;
#pragma warning restore VSTHRD003
    }

    /// <summary>
    /// Starts the web server and runs it until cancellation is requested.
    /// </summary>
    /// <returns>A task, completed when server is stopped.</returns>
    private async Task RunAsync()
    {
        this.Trace("create executor");
        await using (var executor = Executor.Parallel<Server>(Logger))
        {
            this.Trace("start executor");
            executor.Start(_cts.Token);

            this.Trace<string>("handle listener at: {prefixes}", _listener.Prefixes.Select(x => x).Join(","));

            while (!_cts.Token.IsCancellationRequested)
            {
                HttpListenerContext listenerContext;
                try
                {
                    // await for connection
                    listenerContext = await _listener.GetContextAsync().WaitAsync(_cts.Token);
                    this.Trace("socket accepted");
                }
                catch (OperationCanceledException)
                {
                    this.Trace("break, operation canceled");
                    break;
                }

                // try schedule socket handling
                if (executor.Schedule(HandleRequest(listenerContext, _cts.Token)))
                {
                    this.Trace("socket handle scheduled");
                    continue;
                }

                this.Trace("closed and dispose socket (server is already stopping)");
                await CloseConnectionAsync(listenerContext, _cts.Token);
            }
        }

        this.Trace("stop listener");
        _listener.Stop();
    }

    /// <summary>
    /// Creates a function that handles an incoming request by routing it to the appropriate handler.
    /// </summary>
    /// <param name="ctx">The HTTP listener context for the request.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A function that performs the request handling when executed.</returns>
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

    /// <summary>
    /// Handles a WebSocket upgrade request by accepting the connection and delegating to the WebSocket handler.
    /// </summary>
    /// <param name="ctx">The HTTP listener context for the WebSocket upgrade request.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task HandleHttpRequestAsync(HttpListenerContext ctx, CancellationToken ct)
    {
        try
        {
            // normally handler is responsible for closing connection
            this.Trace("handle request");
            await _handleHttp(ctx, ct);
        }
        catch (OperationCanceledException e)
        {
            // if operation canceled - close connection
            this.Trace("handle canceled: {e}. Server stopping: {serverStopping}", e, ct.IsCancellationRequested);
            ctx.Response.StatusCode = ct.IsCancellationRequested ? 503 : 500;
            ctx.Response.Close();
        }
        catch (Exception e)
        {
            this.Trace("handle failed: {e}", e);
            ctx.Response.StatusCode = 500;
            ctx.Response.Close();
        }
        finally
        {
            this.Trace("close normally");
        }

        this.Trace("done");
    }

    /// <summary>
    /// Handles a WebSocket upgrade request by accepting the connection and delegating to the WebSocket handler.
    /// </summary>
    /// <param name="ctx">The HTTP listener context for the WebSocket upgrade request.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task HandleWebSocketRequestAsync(HttpListenerContext ctx, CancellationToken ct)
    {
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

    /// <summary>
    /// Closes an HTTP connection with a 404 Not Found response.
    /// </summary>
    /// <param name="ctx">The HTTP listener context to close.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A completed task.</returns>
    private Task CloseConnectionAsync(HttpListenerContext ctx, CancellationToken ct)
    {
        this.Trace("start");

        try
        {
            ctx.Response.StatusCode = 404;
            ctx.Response.Close();
        }
        catch (Exception e)
        {
            this.Error(e);
        }

        this.Trace("done");

        return Task.CompletedTask;
    }

    /// <summary>
    /// Ignores an Http request by doing nothing (used when no Http handler is configured).
    /// </summary>
    /// <param name="ctx">The Http context to ignore.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A completed task.</returns>
    private Task IgnoreHttpAsync(HttpListenerContext ctx, CancellationToken ct)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Ignores a WebSocket connection by doing nothing (used when no WebSocket handler is configured).
    /// </summary>
    /// <param name="ctx">The WebSocket context to ignore.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A completed task.</returns>
    private Task IgnoreWebSocketAsync(HttpListenerWebSocketContext ctx, CancellationToken ct)
    {
        return Task.CompletedTask;
    }
}
