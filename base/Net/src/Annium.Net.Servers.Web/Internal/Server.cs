using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Execution.Background;
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
    /// The HTTP listener used to accept incoming connections.
    /// </summary>
    private readonly HttpListener _listener;

    /// <summary>
    /// The function used to handle HTTP requests.
    /// </summary>
    private readonly Func<HttpListenerContext, CancellationToken, Task> _handleHttpRequest;

    /// <summary>
    /// The function used to handle WebSocket upgrade requests.
    /// </summary>
    private readonly Func<HttpListenerContext, CancellationToken, Task> _handleWebSocketRequest;

    /// <summary>
    /// The function used to handle established WebSocket connections.
    /// </summary>
    private readonly Func<HttpListenerWebSocketContext, CancellationToken, Task> _handleWebSocket;

    /// <summary>
    /// The executor used for parallel processing of incoming requests.
    /// </summary>
    private readonly IExecutor _executor;

    /// <summary>
    /// Initializes a new instance of the Server class.
    /// </summary>
    /// <param name="port">The port number to listen on.</param>
    /// <param name="httpHandler">The HTTP handler to process HTTP requests, or null to return 404 for all HTTP requests.</param>
    /// <param name="webSocketHandler">The WebSocket handler to process WebSocket connections, or null to reject WebSocket upgrades.</param>
    /// <param name="logger">The logger instance for this server.</param>
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

    /// <summary>
    /// Starts the web server and runs it until cancellation is requested.
    /// </summary>
    /// <param name="ct">The cancellation token to stop the server.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
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

    /// <summary>
    /// Closes an HTTP connection with a 404 Not Found response.
    /// </summary>
    /// <param name="ctx">The HTTP listener context to close.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A completed task.</returns>
    private Task CloseConnectionAsync(HttpListenerContext ctx, CancellationToken ct)
    {
        this.Trace("start");

        ctx.Response.StatusCode = 404;
        ctx.Response.Close();

        this.Trace("done");

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
        this.Trace("done");

        return Task.CompletedTask;
    }
}
