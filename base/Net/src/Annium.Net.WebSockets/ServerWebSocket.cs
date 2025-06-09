using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.WebSockets.Internal;
using NativeWebSocket = System.Net.WebSockets.WebSocket;

namespace Annium.Net.WebSockets;

/// <summary>
/// Server-side WebSocket implementation for handling client connections with connection monitoring capabilities
/// </summary>
public class ServerWebSocket : IServerWebSocket
{
    /// <summary>
    /// Gets the logger instance for this WebSocket server
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Event triggered when a text message is received from a client
    /// </summary>
    public event Action<ReadOnlyMemory<byte>> OnTextReceived = delegate { };

    /// <summary>
    /// Event triggered when a binary message is received from a client
    /// </summary>
    public event Action<ReadOnlyMemory<byte>> OnBinaryReceived = delegate { };

    /// <summary>
    /// Event triggered when the WebSocket connection is closed
    /// </summary>
    public event Action<WebSocketCloseStatus> OnDisconnected = delegate { };

    /// <summary>
    /// Event triggered when an error occurs during WebSocket operations
    /// </summary>
    public event Action<Exception> OnError = delegate { };

    /// <summary>
    /// Lock object for thread-safe status updates
    /// </summary>
    private readonly Lock _locker = new();

    /// <summary>
    /// The underlying managed WebSocket server implementation
    /// </summary>
    private readonly IServerManagedWebSocket _socket;

    /// <summary>
    /// Connection monitor for detecting and handling connection issues
    /// </summary>
    private readonly ConnectionMonitorBase _connectionMonitor;

    /// <summary>
    /// Current connection status of the WebSocket server
    /// </summary>
    private Status _status = Status.Connected;

    /// <summary>
    /// Initializes a new instance of the ServerWebSocket with specified native WebSocket, options, and logger
    /// </summary>
    /// <param name="nativeSocket">The underlying native WebSocket instance</param>
    /// <param name="options">Configuration options for the WebSocket server</param>
    /// <param name="logger">Logger instance for tracing and error reporting</param>
    /// <param name="ct">Cancellation token for the WebSocket operations</param>
    public ServerWebSocket(
        NativeWebSocket nativeSocket,
        ServerWebSocketOptions options,
        ILogger logger,
        CancellationToken ct = default
    )
    {
        Logger = logger;
        this.Trace("start");
        _socket = new ServerManagedWebSocket(nativeSocket, logger, ct);
        _socket.OnTextReceived += HandleOnTextReceived;
        _socket.OnBinaryReceived += HandleOnBinaryReceived;
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
    /// Initializes a new instance of the ServerWebSocket with default options
    /// </summary>
    /// <param name="nativeSocket">The underlying native WebSocket instance</param>
    /// <param name="logger">Logger instance for tracing and error reporting</param>
    /// <param name="ct">Cancellation token for the WebSocket operations</param>
    public ServerWebSocket(NativeWebSocket nativeSocket, ILogger logger, CancellationToken ct = default)
        : this(nativeSocket, ServerWebSocketOptions.Default, logger, ct) { }

    /// <summary>
    /// Disconnects the WebSocket server from the client
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
        _socket.DisconnectAsync().GetAwaiter();

        this.Trace("fire disconnected");
        OnDisconnected(WebSocketCloseStatus.ClosedLocal);

        this.Trace("done");
    }

    /// <summary>
    /// Sends a text message to the client asynchronously
    /// </summary>
    /// <param name="text">The encoded text message to send</param>
    /// <param name="ct">Cancellation token to cancel the operation</param>
    /// <returns>The status of the send operation</returns>
    public ValueTask<WebSocketSendStatus> SendTextAsync(ReadOnlyMemory<byte> text, CancellationToken ct = default)
    {
        this.Trace("send text");
        return _socket.SendTextAsync(text, ct);
    }

    /// <summary>
    /// Sends binary data to the client asynchronously
    /// </summary>
    /// <param name="data">The binary data to send</param>
    /// <param name="ct">Cancellation token to cancel the operation</param>
    /// <returns>The status of the send operation</returns>
    public ValueTask<WebSocketSendStatus> SendBinaryAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        this.Trace("send binary");
        return _socket.SendBinaryAsync(data, ct);
    }

    /// <summary>
    /// Handles the completion of the WebSocket closure and manages cleanup
    /// </summary>
    /// <param name="task">The task representing the closure operation with its result</param>
    private void HandleClosed(Task<WebSocketCloseResult> task)
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
    /// Updates the current connection status with thread-safe logging
    /// </summary>
    /// <param name="status">The new status to set</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetStatus(Status status)
    {
        this.Trace("update status from {oldStatus} to {newStatus}", _status, status);
        _status = status;
    }

    /// <summary>
    /// Handles text messages received from the managed WebSocket and forwards them to event subscribers
    /// </summary>
    /// <param name="data">The received text message data</param>
    private void HandleOnTextReceived(ReadOnlyMemory<byte> data)
    {
        this.Trace("trigger text received");
        OnTextReceived(data);
    }

    /// <summary>
    /// Handles binary messages received from the managed WebSocket and forwards them to event subscribers
    /// </summary>
    /// <param name="data">The received binary message data</param>
    private void HandleOnBinaryReceived(ReadOnlyMemory<byte> data)
    {
        this.Trace("trigger binary received");
        OnBinaryReceived(data);
    }

    /// <summary>
    /// Represents the current connection status of the WebSocket server
    /// </summary>
    private enum Status
    {
        /// <summary>
        /// WebSocket is disconnected
        /// </summary>
        Disconnected,

        /// <summary>
        /// WebSocket is connected and ready for communication
        /// </summary>
        Connected,
    }
}
