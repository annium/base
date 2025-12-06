using System;
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
    /// Uri, that may be used to connect to server
    /// </summary>
    public Uri Uri { get; }

    /// <summary>
    /// TCP listener for accepting incoming connections
    /// </summary>
    private readonly TcpListener _listener;

    /// <summary>
    /// Handler instance for processing incoming connections
    /// </summary>
    private readonly IHandler _handler;

    /// <summary>
    /// Cancellation token source, that will trigger server to stop
    /// </summary>
    private readonly CancellationTokenSource _cts;

    /// <summary>
    /// Task, that will be completed, when server is stopped
    /// </summary>
    private readonly Task _whenStopped;

    /// <summary>
    /// Initializes a new instance of the Server class
    /// </summary>
    /// <param name="listener">TcpListener, server will be working with</param>
    /// <param name="handler">Handler instance for processing connections</param>
    /// <param name="uri">Uri, that will be exposed as base connection address of server</param>
    /// <param name="logger">Logger instance for logging server events</param>
    public Server(TcpListener listener, IHandler handler, Uri uri, ILogger logger)
    {
        Logger = logger;
        Uri = uri;

        _listener = listener;
        _cts = new CancellationTokenSource();
        _handler = handler;
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
    /// Starts and runs the server asynchronously, listening for incoming connections
    /// </summary>
    /// <returns>A task, completed when server is stopped.</returns>
    private async Task RunAsync()
    {
        this.Trace("create executor");
        await using (var executor = Executor.Parallel<Server>(Logger))
        {
            this.Trace("start executor");
            executor.Start(_cts.Token);

            this.Trace("handle listener at: {endpoint}", _listener.LocalEndpoint);

            while (!_cts.Token.IsCancellationRequested)
            {
                Socket socket;
                try
                {
                    // await for connection
                    socket = await _listener.AcceptSocketAsync(_cts.Token);
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
                if (executor.Schedule(HandleSocket(socket, _cts.Token)))
                {
                    this.Trace("socket handle scheduled");
                    continue;
                }

                this.Trace("closed and dispose socket (server is already stopping)");
                socket.Close();
                await socket.DisposeAsync();
            }
        }

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
