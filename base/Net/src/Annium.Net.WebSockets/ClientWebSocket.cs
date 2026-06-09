using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.WebSockets.Internal;

namespace Annium.Net.WebSockets;

/// <summary>
/// Client-side WebSocket implementation with automatic reconnection and connection monitoring capabilities.
/// </summary>
public class ClientWebSocket : IClientWebSocket
{
    /// <summary>
    /// Gets the logger instance for this WebSocket client.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Event triggered when a text message is received from the server.
    /// </summary>
    public event Action<ReadOnlyMemory<byte>> OnTextReceived = delegate { };

    /// <summary>
    /// Event triggered when a binary message is received from the server.
    /// </summary>
    public event Action<ReadOnlyMemory<byte>> OnBinaryReceived = delegate { };

    /// <summary>
    /// Event triggered when the WebSocket connection is successfully established.
    /// </summary>
    public event Action OnConnected = delegate { };

    /// <summary>
    /// Event triggered when the WebSocket connection is closed.
    /// </summary>
    public event Action<WebSocketCloseStatus> OnDisconnected = delegate { };

    /// <summary>
    /// Event triggered when an error occurs during WebSocket operations.
    /// </summary>
    public event Action<Exception> OnError = delegate { };

    /// <summary>
    /// Gets the current connection URI, throws if not set.
    /// </summary>
    private Uri Uri
    {
        get => field ?? throw new InvalidOperationException("Uri is not set");
        set;
    }

    /// <summary>
    /// Lock object for thread-safe status updates.
    /// </summary>
    private readonly Lock _locker = new();

    /// <summary>
    /// The underlying managed WebSocket client implementation.
    /// </summary>
    private readonly IClientManagedWebSocket _socket;

    /// <summary>
    /// Connection monitor for detecting and handling connection issues.
    /// </summary>
    private readonly IConnectionMonitor _connectionMonitor;

    /// <summary>
    /// Timeout in milliseconds for connection attempts.
    /// </summary>
    private readonly int _connectTimeout;

    /// <summary>
    /// Delay in milliseconds before attempting to reconnect after connection loss.
    /// </summary>
    private readonly int _reconnectDelay;

    /// <summary>
    /// Cancellation token source for connection operations.
    /// </summary>
    private CancellationTokenSource _connectionCts = new();

    /// <summary>
    /// Current connection status of the WebSocket client.
    /// </summary>
    private Status _status = Status.Disconnected;

    /// <summary>
    /// True when the WebSocket has finished connecting and not yet started disconnecting. Goes
    /// false synchronously inside <see cref="Disconnect"/> (under <c>_locker</c>) — handlers
    /// firing later from the background teardown observe the disconnected state.
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
    /// Initializes a new instance of the ClientWebSocket with specified options and logger.
    /// </summary>
    /// <param name="options">Configuration options for the WebSocket client.</param>
    /// <param name="logger">Logger instance for tracing and error reporting.</param>
    public ClientWebSocket(ClientWebSocketOptions options, ILogger logger)
    {
        Logger = logger;
        this.Trace("start monitor");
        _socket = new ClientManagedWebSocket(options.KeepAliveInterval, logger);
        _socket.OnTextReceived += HandleOnTextReceived;
        _socket.OnBinaryReceived += HandleOnBinaryReceived;
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
    /// Initializes a new instance of the ClientWebSocket with default options.
    /// </summary>
    /// <param name="logger">Logger instance for tracing and error reporting.</param>
    public ClientWebSocket(ILogger logger)
        : this(ClientWebSocketOptions.Default, logger) { }

    /// <summary>
    /// Connects the WebSocket client to the specified URI.
    /// </summary>
    /// <param name="uri">The URI to connect to.</param>
    public void Connect(Uri uri)
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

        ConnectPrivate(uri);

        this.Trace("done");
    }

    /// <summary>
    /// Disconnects the WebSocket client from the server. The state transition and monitor stop
    /// happen synchronously; the underlying managed-socket teardown and <see cref="OnDisconnected"/>
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
            var oldCts = _connectionCts;
            // replace with a fresh cancelled CTS BEFORE disposing the old one; keeps the
            // invariant that _connectionCts never points to a disposed instance
            var replacement = new CancellationTokenSource();
            replacement.Cancel();
            _connectionCts = replacement;
            oldCts.Cancel();
            oldCts.Dispose();
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
                OnDisconnected(WebSocketCloseStatus.ClosedLocal);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                this.Error("Disconnect background teardown failed: {exception}", ex);
            }
        });

        this.Trace("done");
    }

    /// <summary>
    /// Sends a text message to the server asynchronously.
    /// </summary>
    /// <param name="text">The encoded text message to send.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>The status of the send operation.</returns>
    public ValueTask<WebSocketSendStatus> SendTextAsync(ReadOnlyMemory<byte> text, CancellationToken ct = default)
    {
        this.Trace("send text");
        return _socket.SendTextAsync(text, ct);
    }

    /// <summary>
    /// Sends binary data to the server asynchronously.
    /// </summary>
    /// <param name="data">The binary data to send.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>The status of the send operation.</returns>
    public ValueTask<WebSocketSendStatus> SendBinaryAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        this.Trace("send binary");
        return _socket.SendBinaryAsync(data, ct);
    }

    /// <summary>
    /// Disposes the WebSocket client by triggering <see cref="Disconnect"/> and disposing the connection CTS.
    /// </summary>
    public void Dispose()
    {
        Disconnect();

        // Disconnect() leaves _connectionCts pointing at a fresh, already-cancelled replacement
        // (the "never points to a disposed instance" invariant). Dispose() is terminal: _status is
        // Disconnected, so no Connect/Reconnect path will read the token again — dispose the
        // replacement here to avoid leaking one CTS per socket lifetime.
#pragma warning disable VSTHRD103
        _connectionCts.Dispose();
#pragma warning restore VSTHRD103
    }

    /// <summary>
    /// Handles the reconnection logic after a connection failure or loss.
    /// </summary>
    /// <param name="uri">The URI to reconnect to.</param>
    /// <param name="result">The result from the previous connection attempt or closure.</param>
    private void ReconnectPrivate(Uri uri, WebSocketCloseResult result)
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
        _ = Task.Run(async () =>
        {
            try
            {
                // capture the active token under _locker so the background reconnect task observes a
                // Disconnect() that lands during the delay (the field is rotated under the same lock).
                CancellationToken reconnectCt;
                lock (_locker)
                    reconnectCt = _connectionCts.Token;
                await Task.Delay(_reconnectDelay, reconnectCt);
                this.Trace("trigger connect");
                ConnectPrivate(uri);
                this.Trace("done");
            }
            catch (OperationCanceledException) { }
            // ODE: a terminal Dispose() disposed _connectionCts before/while we read its Token
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                this.Error("scheduled reconnect failed: {exception}", ex);
            }
        });
    }

    /// <summary>
    /// Performs the actual connection logic to the specified URI.
    /// </summary>
    /// <param name="uri">The URI to connect to.</param>
    private void ConnectPrivate(Uri uri)
    {
        this.Trace("start");

        CancellationTokenSource cts;
        lock (_locker)
        {
            if (_status is Status.Disconnected)
            {
                this.Trace("skip - already {status}", _status);
                return;
            }

            Uri = uri;
            // rotate _connectionCts under lock so a racing Disconnect can't double-dispose
            // the CTS between our Interlocked.Exchange and Dispose
            cts = new CancellationTokenSource(_connectTimeout);
            var oldCts = _connectionCts;
            _connectionCts = cts;
            oldCts.Dispose();
        }

        this.Trace("connect to {uri}", uri);
        _ = Task.Run(async () =>
        {
            try
            {
                var task = _socket.ConnectAsync(uri, cts.Token);
                await task.ContinueWith(HandleConnected, uri, CancellationToken.None);
            }
            catch (OperationCanceledException) { }
            // ODE: a concurrent Disconnect()/Dispose() rotated and disposed cts before we read cts.Token
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                this.Error("ConnectPrivate background connect failed: {exception}", ex);
            }
        });

        this.Trace("done");
    }

    /// <summary>
    /// Handles the completion of a connection attempt.
    /// </summary>
    /// <param name="task">The task representing the connection attempt.</param>
    /// <param name="state">The URI state object passed from the connection attempt.</param>
    private void HandleConnected(Task<Exception?> task, object? state)
    {
        this.Trace("start");

        if (task.Exception is not null)
            this.Error(task.Exception);

        // VSTHRD002: task.Result is safe — HandleConnected runs only as a ContinueWith callback on a completed antecedent.
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
            // state! is safe: it is always the non-null Uri passed as the ContinueWith state argument in ConnectPrivate.
            var uri = (Uri)state!;
            this.Trace("failure: {exception}, init reconnect", task.Exception);
            ReconnectPrivate(uri, new WebSocketCloseResult(WebSocketCloseStatus.Error, task.Result));
            return;
        }
#pragma warning restore VSTHRD002

        this.Trace("subscribe to IsClosed");
        _ = _socket.IsClosed.ContinueWith(
            task =>
            {
                try
                {
                    HandleClosed(task);
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    this.Error("HandleClosed failed: {exception}", ex);
                }
            },
            CancellationToken.None
        );

        this.Trace("start monitor");
        _connectionMonitor.Start();

        this.Trace("fire connected");
        OnConnected();

        this.Trace("done");
    }

    /// <summary>
    /// Handles the event when the connection monitor detects a lost connection. Kicks off the
    /// managed socket disconnect in fire-and-forget mode because the monitor's event is
    /// synchronous; exceptions surface through <see cref="Logger"/>.
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
        _ = Task.Run(async () =>
        {
            try
            {
                await _socket.DisconnectAsync();
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                this.Error("HandleConnectionLost disconnect failed: {exception}", ex);
            }
        });

        this.Trace("done");
    }

    /// <summary>
    /// Handles the completion of the WebSocket closure.
    /// </summary>
    /// <param name="task">The task representing the closure operation with its result.</param>
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

            SetStatus(Status.Connecting);
        }

        // VSTHRD002: task.Result is safe — HandleClosed runs only as a ContinueWith callback on a completed antecedent.
#pragma warning disable VSTHRD002
        ReconnectPrivate(Uri, task.Result);
#pragma warning restore VSTHRD002

        this.Trace("done");
    }

    /// <summary>
    /// Updates the current connection status with thread-safe logging.
    /// </summary>
    /// <param name="status">The new status to set.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetStatus(Status status)
    {
        this.Trace("update status from {currentStatus} to {newStatus}", _status, status);
        _status = status;
    }

    /// <summary>
    /// Handles text messages received from the managed WebSocket and forwards them to event subscribers.
    /// </summary>
    /// <param name="data">The received text message data.</param>
    private void HandleOnTextReceived(ReadOnlyMemory<byte> data) => OnTextReceived(data);

    /// <summary>
    /// Handles binary messages received from the managed WebSocket and forwards them to event subscribers.
    /// Filters out ping protocol frames so they don't surface as application data.
    /// </summary>
    /// <param name="data">The received binary message data.</param>
    private void HandleOnBinaryReceived(ReadOnlyMemory<byte> data)
    {
        if (ProtocolFrames.IsPingFrame(data))
            return;
        OnBinaryReceived(data);
    }

    /// <summary>
    /// Represents the current connection status of the WebSocket client.
    /// </summary>
    private enum Status
    {
        /// <summary>
        /// WebSocket is disconnected.
        /// </summary>
        Disconnected,

        /// <summary>
        /// WebSocket is in the process of connecting.
        /// </summary>
        Connecting,

        /// <summary>
        /// WebSocket is connected and ready for communication.
        /// </summary>
        Connected,
    }
}
