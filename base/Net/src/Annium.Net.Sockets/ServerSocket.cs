using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.Sockets.Internal;

namespace Annium.Net.Sockets;

/// <summary>
/// Implementation of a server-side socket that handles communication with a connected client
/// </summary>
public class ServerSocket : IServerSocket
{
    /// <summary>
    /// Gets the logger instance
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Event raised when binary data is received from the client
    /// </summary>
    public event Action<ReadOnlyMemory<byte>> OnReceived = delegate { };

    /// <summary>
    /// Event raised when the socket is disconnected from the client
    /// </summary>
    public event Action<SocketCloseStatus> OnDisconnected = delegate { };

    /// <summary>
    /// Event raised when an error occurs during socket operations
    /// </summary>
    public event Action<Exception> OnError = delegate { };

    /// <summary>
    /// Thread synchronization lock for socket operations
    /// </summary>
    private readonly Lock _locker = new();

    /// <summary>
    /// The underlying managed socket for actual network operations
    /// </summary>
    private readonly IServerManagedSocket _socket;

    /// <summary>
    /// Connection monitor for health checking and connection management
    /// </summary>
    private readonly ConnectionMonitorBase _connectionMonitor;

    /// <summary>
    /// Current connection status of the socket
    /// </summary>
    private Status _status = Status.Connected;

    /// <summary>
    /// Initializes a new instance of the ServerSocket class with specified options
    /// </summary>
    /// <param name="stream">The network stream for communication</param>
    /// <param name="options">Configuration options for the socket</param>
    /// <param name="logger">Logger instance for diagnostics</param>
    /// <param name="ct">Cancellation token for the socket lifetime</param>
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
        _socket.IsClosed.ContinueWith(HandleClosed, CancellationToken.None).GetAwaiter();

        this.Trace("init monitor");
        _connectionMonitor =
            options.ConnectionMonitor.Factory?.Create(_socket, options.ConnectionMonitor)
            ?? new NoneConnectionMonitor(Logger);

        this.Trace("start monitor");
        _connectionMonitor.Start();

        this.Trace("subscribe to OnConnectionLost");
        _connectionMonitor.OnConnectionLost += Disconnect;
    }

    /// <summary>
    /// Initializes a new instance of the ServerSocket class with default options
    /// </summary>
    /// <param name="stream">The network stream for communication</param>
    /// <param name="logger">Logger instance for diagnostics</param>
    /// <param name="ct">Cancellation token for the socket lifetime</param>
    public ServerSocket(Stream stream, ILogger logger, CancellationToken ct = default)
        : this(stream, ServerSocketOptions.Default, logger, ct) { }

    /// <summary>
    /// Disconnects from the client
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
        _socket
            .DisconnectAsync()
            .ContinueWith(_ =>
            {
                this.Trace("fire disconnected");
                OnDisconnected(SocketCloseStatus.ClosedLocal);
            })
            .GetAwaiter();

        this.Trace("done");
    }

    /// <summary>
    /// Sends binary data to the client asynchronously
    /// </summary>
    /// <param name="data">The data to send</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The status of the send operation</returns>
    public ValueTask<SocketSendStatus> SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        this.Trace("send");
        return _socket.SendAsync(data, ct);
    }

    /// <summary>
    /// Disposes the socket and releases all resources
    /// </summary>
    public void Dispose()
    {
        Disconnect();
    }

    /// <summary>
    /// Handles when the underlying socket is closed
    /// </summary>
    /// <param name="task">The socket close task result</param>
    private void HandleClosed(Task<SocketCloseResult> task)
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

#pragma warning disable VSTHRD002
        var result = task.Result;
#pragma warning restore VSTHRD002
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
    /// Updates the internal connection status
    /// </summary>
    /// <param name="status">The new status to set</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetStatus(Status status)
    {
        this.Trace("update status from {oldStatus} to {newStatus}", _status, status);
        _status = status;
    }

    /// <summary>
    /// Handles received data from the underlying socket, filtering out protocol frames
    /// </summary>
    /// <param name="data">The received data</param>
    private void HandleOnReceived(ReadOnlyMemory<byte> data)
    {
        if (data.Span.SequenceEqual(ProtocolFrames.Ping.Span))
        {
            this.Trace("skip ping frame");
            return;
        }

        this.Trace("trigger binary received");
        OnReceived(data);
    }

    /// <summary>
    /// Internal connection status
    /// </summary>
    private enum Status
    {
        /// <summary>
        /// Socket is disconnected
        /// </summary>
        Disconnected,

        /// <summary>
        /// Socket is connected and ready for communication
        /// </summary>
        Connected,
    }
}
