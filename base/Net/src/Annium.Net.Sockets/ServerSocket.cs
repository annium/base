using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.Sockets.Internal;

namespace Annium.Net.Sockets;

/// <summary>
/// Implementation of a server-side socket that handles communication with a connected client.
/// </summary>
public class ServerSocket : IServerSocket
{
    /// <summary>
    /// Gets the logger instance.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Event raised when binary data is received from the client.
    /// </summary>
    public event Action<ReadOnlyMemory<byte>> OnReceived = delegate { };

    /// <summary>
    /// Event raised when the socket is disconnected from the client.
    /// </summary>
    public event Action<SocketCloseStatus> OnDisconnected = delegate { };

    /// <summary>
    /// Event raised when an error occurs during socket operations.
    /// </summary>
    public event Action<Exception> OnError = delegate { };

    /// <summary>
    /// Indicates whether the socket is currently connected to the client. Goes false synchronously
    /// (under <c>_locker</c>) when <see cref="Disconnect"/> begins or the connection is closed, so
    /// handlers firing later from the background teardown observe the disconnected state.
    /// </summary>
    public bool IsConnected
    {
        get
        {
            lock (_locker)
                return _status == Status.Connected;
        }
    }

    /// <summary>
    /// Thread synchronization lock for socket operations.
    /// </summary>
    private readonly Lock _locker = new();

    /// <summary>
    /// The underlying managed socket for actual network operations.
    /// </summary>
    private readonly IServerManagedSocket _socket;

    /// <summary>
    /// Connection monitor for health checking and connection management.
    /// </summary>
    private readonly IConnectionMonitor _connectionMonitor;

    /// <summary>
    /// Current connection status of the socket.
    /// </summary>
    private Status _status = Status.Connected;

    /// <summary>
    /// Initializes a new instance of the ServerSocket class with specified options.
    /// </summary>
    /// <param name="stream">The network stream for communication.</param>
    /// <param name="options">Configuration options for the socket.</param>
    /// <param name="logger">Logger instance for diagnostics.</param>
    /// <param name="ct">Cancellation token for the socket lifetime.</param>
    public ServerSocket(Stream stream, ServerSocketOptions options, ILogger logger, CancellationToken ct = default)
    {
        Logger = logger;
        this.Trace("start");
        var managedOptions = new ManagedSocketOptions
        {
            Mode = options.Mode,
            BufferSize = options.BufferSize,
            ExtremeMessageSize = options.ExtremeMessageSize,
        };
        _socket = new ServerManagedSocket(stream, managedOptions, logger, ct);
        _socket.OnReceived += HandleOnReceived;
        this.Trace<string>("paired with {socket}", _socket.GetFullId());

        this.Trace("subscribe to IsClosed");
#pragma warning disable VSTHRD003
        _ = _socket.IsClosed.ContinueWith(
            async task =>
            {
                try
                {
                    await HandleClosedAsync(task);
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    this.Error("HandleClosed failed: {exception}", ex);
                }
            },
            CancellationToken.None
        );
#pragma warning restore VSTHRD003

        this.Trace("init monitor");
        _connectionMonitor =
            options.ConnectionMonitor.Factory?.Create(_socket, options.ConnectionMonitor)
            ?? new NoneConnectionMonitor(Logger);

        // subscribe before Start so the monitor's first FireConnectionLost can never fire into the
        // void — Start may begin emitting connection-lost events synchronously from its timer.
        this.Trace("subscribe to OnConnectionLost");
        _connectionMonitor.OnConnectionLost += Disconnect;

        this.Trace("start monitor");
        _connectionMonitor.Start();
    }

    /// <summary>
    /// Initializes a new instance of the ServerSocket class with default options.
    /// </summary>
    /// <param name="stream">The network stream for communication.</param>
    /// <param name="logger">Logger instance for diagnostics.</param>
    /// <param name="ct">Cancellation token for the socket lifetime.</param>
    public ServerSocket(Stream stream, ILogger logger, CancellationToken ct = default)
        : this(stream, ServerSocketOptions.Default, logger, ct) { }

    /// <summary>
    /// Disconnects from the client. The state transition and monitor stop happen
    /// synchronously; the underlying managed-socket teardown and <see cref="OnDisconnected"/>
    /// event fire on a background task so that the event is raised only after the teardown
    /// completes. Callers that need to observe completion can subscribe to
    /// <see cref="OnDisconnected"/> or use the <c>WhenDisconnectedAsync</c> extension.
    /// </summary>
    public void Disconnect()
    {
        this.Trace("start");

        lock (_locker)
        {
            if (_status is Status.Disconnected)
            {
                this.Trace("skip - already {status}", _status);
                return;
            }

            SetStatus(Status.Disconnected);
        }

        this.Trace("stop monitor");
        _connectionMonitor.Stop();

        this.Trace("disconnect managed socket");
        _ = Task.Run(async () =>
        {
            try
            {
                await _socket.DisconnectAsync();

                this.Trace("fire disconnected");
                OnDisconnected(SocketCloseStatus.ClosedLocal);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                this.Error<string>("background disconnect teardown failed: {error}", ex.ToString());
            }
        });

        this.Trace("done");
    }

    /// <summary>
    /// Sends binary data to the client asynchronously.
    /// </summary>
    /// <param name="data">The data to send.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The status of the send operation.</returns>
    public ValueTask<SocketSendStatus> SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        this.Trace("send");
        return _socket.SendAsync(data, ct);
    }

    /// <summary>
    /// Disposes the socket with **forced-close** semantics: stops the connection monitor and
    /// synchronously disposes the underlying managed socket so the stream is closed
    /// deterministically rather than via the GC finalizer. Does NOT trigger a graceful disconnect
    /// — callers wanting graceful close should call <see cref="Disconnect"/> first and wait for
    /// <see cref="OnDisconnected"/> to fire (or use the <c>WhenDisconnectedAsync</c> extension)
    /// before invoking <see cref="Dispose"/>. Mixing graceful and forced close in a single
    /// <see cref="Dispose"/> body would race the synchronous teardown against the fire-and-forget
    /// <see cref="Disconnect"/> background task.
    /// </summary>
    public void Dispose()
    {
        // Stop the monitor first: IConnectionMonitor.Stop() is synchronous and idempotent and
        // only halts the monitor's ping timer (it never touches the socket), so it does not
        // introduce the graceful-vs-forced race described above. Without it, disposing without a
        // prior Disconnect() leaks the monitor timer, which keeps pinging the dead socket.
        _connectionMonitor.Stop();

        _socket.Dispose();
    }

    /// <summary>
    /// Handles when the underlying socket is closed.
    /// </summary>
    /// <param name="task">The socket close task result.</param>
    /// <returns>A task that completes after subscribers have been notified.</returns>
    private async Task HandleClosedAsync(Task<SocketCloseResult> task)
    {
        this.Trace("start");

        if (task.Exception is not null)
            this.Error(task.Exception);

        lock (_locker)
        {
            if (_status is Status.Disconnected)
            {
                this.Trace("skip - already {status}", _status);
                return;
            }

            SetStatus(Status.Disconnected);
        }

        this.Trace("stop monitor");
        _connectionMonitor.Stop();

#pragma warning disable VSTHRD003
        var result = await task;
#pragma warning restore VSTHRD003
        if (result.Exception is not null)
        {
            this.Trace("fire error: {exception}", result.Exception);
            OnError(result.Exception);
        }

        this.Trace("fire disconnected");
        OnDisconnected(result.Status);

        this.Trace("done");
    }

    /// <summary>
    /// Updates the internal connection status.
    /// </summary>
    /// <param name="status">The new status to set.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetStatus(Status status)
    {
        this.Trace("update status from {oldStatus} to {newStatus}", _status, status);
        _status = status;
    }

    /// <summary>
    /// Handles received data from the underlying socket, filtering out protocol frames.
    /// </summary>
    /// <param name="data">The received data.</param>
    private void HandleOnReceived(ReadOnlyMemory<byte> data)
    {
        if (ProtocolFrames.IsPingFrame(data))
        {
            this.Trace("skip ping frame");
            return;
        }

        this.Trace("trigger binary received");
        OnReceived(data);
    }

    /// <summary>
    /// Internal connection status.
    /// </summary>
    private enum Status
    {
        /// <summary>
        /// Socket is disconnected.
        /// </summary>
        Disconnected,

        /// <summary>
        /// Socket is connected and ready for communication.
        /// </summary>
        Connected,
    }
}
