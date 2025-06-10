using System;
using System.Net;
using System.Net.Security;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.Sockets.Internal;

namespace Annium.Net.Sockets;

/// <summary>
/// Implementation of a client socket that can connect to remote endpoints and automatically reconnect on connection loss
/// </summary>
public class ClientSocket : IClientSocket
{
    /// <summary>
    /// Gets the logger instance
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Event raised when binary data is received from the remote endpoint
    /// </summary>
    public event Action<ReadOnlyMemory<byte>> OnReceived = delegate { };

    /// <summary>
    /// Event raised when the socket successfully connects to a remote endpoint
    /// </summary>
    public event Action OnConnected = delegate { };

    /// <summary>
    /// Event raised when the socket is disconnected from the remote endpoint
    /// </summary>
    public event Action<SocketCloseStatus> OnDisconnected = delegate { };

    /// <summary>
    /// Event raised when an error occurs during socket operations
    /// </summary>
    public event Action<Exception> OnError = delegate { };

    /// <summary>
    /// Gets the current connection configuration
    /// </summary>
    private ConnectionConfig Config =>
        _connectionConfig ?? throw new InvalidOperationException("Connection config is not set");

    /// <summary>
    /// Thread synchronization lock for connection operations
    /// </summary>
    private readonly Lock _locker = new();

    /// <summary>
    /// The underlying managed socket for actual network operations
    /// </summary>
    private readonly IClientManagedSocket _socket;

    /// <summary>
    /// Connection monitor for health checking and reconnection logic
    /// </summary>
    private readonly ConnectionMonitorBase _connectionMonitor;

    /// <summary>
    /// Timeout for connection attempts in milliseconds
    /// </summary>
    private readonly int _connectTimeout;

    /// <summary>
    /// Delay between reconnection attempts in milliseconds
    /// </summary>
    private readonly int _reconnectDelay;

    /// <summary>
    /// Current connection configuration including endpoint and SSL settings
    /// </summary>
    private ConnectionConfig? _connectionConfig;

    /// <summary>
    /// Cancellation token source for managing connection operations
    /// </summary>
    private CancellationTokenSource _connectionCts = new();

    /// <summary>
    /// Current connection status of the socket
    /// </summary>
    private Status _status = Status.Disconnected;

    /// <summary>
    /// Initializes a new instance of the ClientSocket class with specified options
    /// </summary>
    /// <param name="options">Configuration options for the socket</param>
    /// <param name="logger">Logger instance for diagnostics</param>
    public ClientSocket(ClientSocketOptions options, ILogger logger)
    {
        Logger = logger;
        this.Trace("start monitor");
        var managedOptions = new ManagedSocketOptions
        {
            Mode = options.Mode,
            BufferSize = options.BufferSize,
            ExtremeMessageSize = options.ExtremeMessageSize,
        };
        _socket = new ClientManagedSocket(managedOptions, logger);
        _socket.OnReceived += HandleOnReceived;
        this.Trace<string>("paired with {socket}", _socket.GetFullId());

        this.Trace("init monitor");
        _connectionMonitor =
            options.ConnectionMonitor.Factory?.Create(_socket, options.ConnectionMonitor)
            ?? new NoneConnectionMonitor(Logger);

        _connectTimeout = options.ConnectTimeout;
        _reconnectDelay = options.ReconnectDelay;

        this.Trace("subscribe to OnConnectionLost");
        _connectionMonitor.OnConnectionLost += HandleConnectionLost;
    }

    /// <summary>
    /// Initializes a new instance of the ClientSocket class with default options
    /// </summary>
    /// <param name="logger">Logger instance for diagnostics</param>
    public ClientSocket(ILogger logger)
        : this(ClientSocketOptions.Default, logger) { }

    /// <summary>
    /// Connects to the specified remote endpoint
    /// </summary>
    /// <param name="endpoint">The remote endpoint to connect to</param>
    /// <param name="authOptions">Optional SSL client authentication options for secure connections</param>
    public void Connect(IPEndPoint endpoint, SslClientAuthenticationOptions? authOptions = null)
    {
        this.Trace("start");

        lock (_locker)
        {
            if (_status is Status.Connecting or Status.Connected)
            {
                this.Trace("skip - already {status}", _status);
                return;
            }

            SetStatus(Status.Connecting);
        }

        ConnectPrivate(new(endpoint, authOptions));

        this.Trace("done");
    }

    /// <summary>
    /// Disconnects from the remote endpoint
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
            _connectionCts.Cancel();
            _connectionCts.Dispose();
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
    /// Sends binary data to the remote endpoint asynchronously
    /// </summary>
    /// <param name="data">The data to send</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The status of the send operation</returns>
    public ValueTask<SocketSendStatus> SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        this.Trace("send binary");
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
    /// Handles reconnection logic after a connection is lost
    /// </summary>
    /// <param name="config">The connection configuration to use for reconnection</param>
    /// <param name="result">The result of the previous connection close</param>
    private void ReconnectPrivate(ConnectionConfig config, SocketCloseResult result)
    {
        this.Trace("start");

        this.Trace("stop monitor");
        _connectionMonitor.Stop();

        if (result.Exception is not null)
        {
            this.Trace("fire error: {exception}", result.Exception);
            OnError(result.Exception);
        }

        this.Trace("fire disconnected with {closeStatus}", result.Status);
        OnDisconnected(result.Status);

        this.Trace("schedule connection in {reconnectDelay}ms", _reconnectDelay);
        Task.Delay(_reconnectDelay)
            .ContinueWith(_ =>
            {
                this.Trace("trigger connect");
                ConnectPrivate(config);

                this.Trace("done");
            })
            .GetAwaiter();
    }

    /// <summary>
    /// Performs the actual connection logic
    /// </summary>
    /// <param name="config">The connection configuration</param>
    private void ConnectPrivate(ConnectionConfig config)
    {
        this.Trace("start");

        lock (_locker)
        {
            if (_status is Status.Disconnected)
            {
                this.Trace("skip - already {status}", _status);
                return;
            }
        }

        _connectionConfig = config;
        this.Trace<IPEndPoint, string>(
            "connect to {endpoint} ({ssl})",
            config.Endpoint,
            config.AuthOptions is not null ? "ssl" : "plaintext"
        );
        var cts = new CancellationTokenSource(_connectTimeout);
        _connectionCts = cts;
        _socket
            .ConnectAsync(config.Endpoint, config.AuthOptions, cts.Token)
            .ContinueWith(HandleConnected, config, CancellationToken.None)
            .GetAwaiter();

        this.Trace("done");
    }

    /// <summary>
    /// Handles the result of a connection attempt
    /// </summary>
    /// <param name="task">The connection task result</param>
    /// <param name="state">The connection configuration state</param>
    private void HandleConnected(Task<Exception?> task, object? state)
    {
        this.Trace("start");

        if (task.Exception is not null)
            this.Error(task.Exception);

#pragma warning disable VSTHRD002
        lock (_locker)
        {
            if (_status is Status.Connected or Status.Disconnected)
            {
                this.Trace("skip - already {status}", _status);
                return;
            }

            // set status in lock
            this.Trace<string>(
                "set status by connection result: {result}",
                task.Result is null ? "ok" : task.Result.ToString()
            );
            SetStatus(task.Result is null ? Status.Connected : Status.Connecting);
        }

        if (task.Result is not null)
        {
            var config = (ConnectionConfig)state!;
            this.Trace("failure: {exception}, init reconnect", task.Exception);
            ReconnectPrivate(config, new SocketCloseResult(SocketCloseStatus.Error, task.Result));
            return;
        }
#pragma warning restore VSTHRD002

        this.Trace("subscribe to IsClosed");
        _socket.IsClosed.ContinueWith(HandleClosed, CancellationToken.None).GetAwaiter();

        this.Trace("start monitor");
        _connectionMonitor.Start();

        this.Trace("fire connected");
        OnConnected();

        this.Trace("done");
    }

    /// <summary>
    /// Handles when the connection monitor detects a lost connection
    /// </summary>
    private void HandleConnectionLost()
    {
        this.Trace("start");

        lock (_locker)
        {
            if (_status is Status.Disconnected)
            {
                this.Trace("skip - already {status}", _status);
                return;
            }

            SetStatus(Status.Connecting);
        }

        this.Trace("disconnect managed socket");
        _socket.DisconnectAsync().GetAwaiter();

        this.Trace("done");
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

            SetStatus(Status.Connecting);
        }

#pragma warning disable VSTHRD002
        ReconnectPrivate(Config, task.Result);
#pragma warning restore VSTHRD002

        this.Trace("done");
    }

    /// <summary>
    /// Updates the internal connection status
    /// </summary>
    /// <param name="status">The new status to set</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetStatus(Status status)
    {
        this.Trace("update status from {currentStatus} to {newStatus}", _status, status);
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
    /// Configuration for a socket connection
    /// </summary>
    /// <param name="Endpoint">The remote endpoint to connect to</param>
    /// <param name="AuthOptions">Optional SSL authentication options</param>
    private record ConnectionConfig(IPEndPoint Endpoint, SslClientAuthenticationOptions? AuthOptions);

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
        /// Socket is in the process of connecting
        /// </summary>
        Connecting,

        /// <summary>
        /// Socket is connected and ready for communication
        /// </summary>
        Connected,
    }
}
