using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Execution.Background;
using Annium.Logging;

namespace Annium.Net.Servers.Sockets.Internal;

/// <summary>
/// Internal implementation of a TCP socket server that accepts connections and handles them using a configured handler
/// </summary>
internal class Server : IServer, ILogSubject
{
    /// <summary>
    /// Gets the logger instance for this server
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// TCP listener for accepting incoming connections
    /// </summary>
    private readonly TcpListener _listener;

    /// <summary>
    /// Background executor for handling connections in parallel
    /// </summary>
    private readonly IExecutor _executor;

    /// <summary>
    /// Handler instance for processing incoming connections
    /// </summary>
    private readonly IHandler _handler;

    /// <summary>
    /// Thread-safe flag indicating whether the server is currently listening
    /// </summary>
    private int _isListening;

    /// <summary>
    /// Initializes a new instance of the Server class
    /// </summary>
    /// <param name="port">Port number to listen on</param>
    /// <param name="handler">Handler instance for processing connections</param>
    /// <param name="logger">Logger instance for logging server events</param>
    public Server(int port, IHandler handler, ILogger logger)
    {
        Logger = logger;
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Server.NoDelay = true;
        _executor = Executor.Parallel<Server>(Logger);
        _handler = handler;
    }

    /// <summary>
    /// Starts and runs the server asynchronously, listening for incoming connections
    /// </summary>
    /// <param name="ct">Cancellation token to stop the server</param>
    /// <returns>A task that represents the asynchronous server operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the server is already running</exception>
    public async Task RunAsync(CancellationToken ct = default)
    {
        this.Trace("start");

        if (Interlocked.CompareExchange(ref _isListening, 1, 0) == 1)
            throw new InvalidOperationException("Server is already started");

        this.Trace("start executor");
        _executor.Start(ct);

        this.Trace("start listener");
        _listener.Start();

        while (!ct.IsCancellationRequested)
        {
            Socket socket;
            try
            {
                // await for connection
                socket = await _listener.AcceptSocketAsync(ct);
                socket.NoDelay = true;
                socket.LingerState = new LingerOption(true, 0);
                this.Trace("socket accepted");
            }
            catch (OperationCanceledException)
            {
                this.Trace("break, operation canceled");
                break;
            }

            // try schedule socket handling
            if (_executor.Schedule(HandleSocket(socket, ct)))
            {
                this.Trace("socket handle scheduled");
                continue;
            }

            this.Trace("closed and dispose socket (server is already stopping)");
            socket.Close();
            await socket.DisposeAsync();
        }

        // when cancelled - await connections processing and stop listener
        this.Trace("dispose executor");
        await _executor.DisposeAsync().ConfigureAwait(false);

        this.Trace("stop listener");
        _listener.Stop();
    }

    /// <summary>
    /// Creates a function that handles a socket connection asynchronously
    /// </summary>
    /// <param name="socket">The socket to handle</param>
    /// <param name="ct">Cancellation token for the operation</param>
    /// <returns>A function that returns a ValueTask for handling the socket</returns>
    private Func<ValueTask> HandleSocket(Socket socket, CancellationToken ct) =>
        async () =>
        {
            try
            {
                this.Trace("handle socket");
                await _handler.HandleAsync(socket, ct).ConfigureAwait(false);
            }
            finally
            {
                if (socket.Connected)
                {
                    this.Trace("close socket");
                    socket.Close();
                }
                else
                    this.Trace("socket is not connected already");

                this.Trace("dispose socket");
                await socket.DisposeAsync();
            }
        };
}
