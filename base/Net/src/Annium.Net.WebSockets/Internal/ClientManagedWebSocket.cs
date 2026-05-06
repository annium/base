using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using NativeWebSocket = System.Net.WebSockets.ClientWebSocket;

namespace Annium.Net.WebSockets.Internal;

/// <summary>
/// Client-side managed WebSocket implementation that wraps the native WebSocket with additional functionality.
/// </summary>
internal class ClientManagedWebSocket : IClientManagedWebSocket, ILogSubject
{
    /// <summary>
    /// Gets the logger instance for this managed WebSocket.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Event triggered when a text message is received.
    /// </summary>
    public event Action<ReadOnlyMemory<byte>> OnTextReceived = delegate { };

    /// <summary>
    /// Event triggered when a binary message is received.
    /// </summary>
    public event Action<ReadOnlyMemory<byte>> OnBinaryReceived = delegate { };

    /// <summary>
    /// Gets a task that completes when the WebSocket is closed.
    /// </summary>
    public Task<WebSocketCloseResult> IsClosed { get; private set; } =
        Task.FromResult(new WebSocketCloseResult(WebSocketCloseStatus.ClosedLocal, null));

    /// <summary>
    /// Keep-alive interval in milliseconds for the WebSocket connection.
    /// </summary>
    private readonly int _keepAliveInterval;

    /// <summary>
    /// Lock object for thread-safe operations.
    /// </summary>
    private readonly Lock _locker = new();

    /// <summary>
    /// Current active connection, null if not connected.
    /// </summary>
    private Connection? _cn;

    /// <summary>
    /// Cancellation token source for listening operations.
    /// </summary>
    private CancellationTokenSource _listenCts = new();

    /// <summary>
    /// Initializes a new instance of the ClientManagedWebSocket.
    /// </summary>
    /// <param name="keepAliveInterval">Keep-alive interval in milliseconds.</param>
    /// <param name="logger">Logger instance for tracing.</param>
    public ClientManagedWebSocket(int keepAliveInterval, ILogger logger)
    {
        _keepAliveInterval = keepAliveInterval;
        Logger = logger;
    }

    /// <summary>
    /// Disposes the managed WebSocket and releases all resources.
    /// </summary>
    public void Dispose()
    {
        this.Trace("start");

        var cn = TeardownUnderLock();
#pragma warning disable VSTHRD103
        cn?.Dispose();
#pragma warning restore VSTHRD103

        this.Trace("done");
    }

    /// <summary>
    /// Connects to the specified URI asynchronously.
    /// </summary>
    /// <param name="uri">The URI to connect to.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Null if successful, otherwise the exception that occurred.</returns>
    public async Task<Exception?> ConnectAsync(Uri uri, CancellationToken ct = default)
    {
        this.Trace("start");

        // only connection is checked, because after disconnect listen task can still be awaited
        if (_cn is not null)
            throw new InvalidOperationException("Socket is already connected");

        var nativeSocket = new NativeWebSocket
        {
            Options = { KeepAliveInterval = TimeSpan.FromMilliseconds(_keepAliveInterval) },
        };
        var managedSocket = new ManagedWebSocket(nativeSocket, Logger);
        this.Trace<string, string>(
            "paired with {nativeSocket} / {managedSocket}",
            nativeSocket.GetFullId(),
            managedSocket.GetFullId()
        );

        this.Trace("bind events");
        managedSocket.OnTextReceived += HandleOnTextReceived;
        managedSocket.OnBinaryReceived += HandleOnBinaryReceived;

        try
        {
            this.Trace("connect native socket to {uri}", uri);
            await nativeSocket.ConnectAsync(uri, ct);

            var cn = new Connection(nativeSocket, managedSocket, Logger);

            lock (_locker)
            {
                if (ct.IsCancellationRequested)
                {
                    this.Trace("connection canceled, dispose");
#pragma warning disable VSTHRD103
                    cn.Dispose();
#pragma warning restore VSTHRD103

                    return null;
                }

                this.Trace("save connection");
                _cn = cn;

                this.Trace("create listen cts");
                // dispose the outgoing CTS before installing the new one (the field initializer
                // creates an instance that is otherwise leaked on first connect).
                var oldListenCts = _listenCts;
                _listenCts = new CancellationTokenSource();
#pragma warning disable VSTHRD103
                oldListenCts.Dispose();
#pragma warning restore VSTHRD103

                this.Trace("create listen task");
                IsClosed = managedSocket
                    .ListenAsync(_listenCts.Token)
                    .ContinueWith(
                        HandleClosed,
                        CancellationToken.None,
                        TaskContinuationOptions.ExecuteSynchronously,
                        TaskScheduler.Default
                    );
            }

            this.Trace("done (connected)");

            return null;
        }
        catch (Exception e)
        {
            this.Trace("failed: {e}", e);

            Cleanup(nativeSocket, managedSocket);

            this.Trace("done (not connected)");

            return e;
        }
    }

    /// <summary>
    /// Disconnects the WebSocket asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous disconnect operation.</returns>
    public async Task DisconnectAsync()
    {
        this.Trace("start");

        var cn = TeardownUnderLock();
        if (cn is null)
            return;

        try
        {
            this.Trace("close output");
            if (cn.Native.State is WebSocketState.Open or WebSocketState.CloseReceived)
                await cn.Native.CloseOutputAsync(
                    System.Net.WebSockets.WebSocketCloseStatus.NormalClosure,
                    string.Empty,
                    CancellationToken.None
                );
        }
        catch (Exception e)
        {
            this.Trace("failed: {e}", e);
        }

        this.Trace("await listen task");
#pragma warning disable VSTHRD003
        await IsClosed;
#pragma warning restore VSTHRD003

        // dispose the connection now that the close-output handshake is done and the listen
        // loop has terminated. Without this, every clean DisconnectAsync leaks the native
        // ClientWebSocket — only the GC finalizer would reclaim it.
#pragma warning disable VSTHRD103
        cn.Dispose();
#pragma warning restore VSTHRD103

        this.Trace("done");
    }

    /// <summary>
    /// Performs the synchronous teardown shared by <see cref="Dispose"/> and <see cref="DisconnectAsync"/>:
    /// claim the live connection under <c>_locker</c>, unbind events, cancel + dispose the listen CTS.
    /// </summary>
    /// <returns>The torn-down connection, or null if the socket was not connected. <c>DisconnectAsync</c>
    /// uses the returned <c>Native</c> reference to send the close-output frame.</returns>
    private Connection? TeardownUnderLock()
    {
        lock (_locker)
        {
            var cn = Interlocked.Exchange(ref _cn, null);
            if (cn is null)
            {
                this.Trace("skip - not connected");
                return null;
            }

            this.Trace("unbind events");
            cn.Managed.OnTextReceived -= HandleOnTextReceived;
            cn.Managed.OnBinaryReceived -= HandleOnBinaryReceived;

            this.Trace("cancel listen cts");
#pragma warning disable VSTHRD103
            _listenCts.Cancel();
            _listenCts.Dispose();
#pragma warning restore VSTHRD103

            return cn;
        }
    }

    /// <summary>
    /// Sends a text message asynchronously.
    /// </summary>
    /// <param name="text">The encoded text to send.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The status of the send operation.</returns>
    public ValueTask<WebSocketSendStatus> SendTextAsync(ReadOnlyMemory<byte> text, CancellationToken ct = default)
    {
        this.Trace("send text");

        return _cn?.Managed.SendTextAsync(text, ct) ?? ValueTask.FromResult(WebSocketSendStatus.Closed);
    }

    /// <summary>
    /// Sends binary data asynchronously.
    /// </summary>
    /// <param name="data">The binary data to send.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The status of the send operation.</returns>
    public ValueTask<WebSocketSendStatus> SendBinaryAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        this.Trace("send binary");

        return _cn?.Managed.SendBinaryAsync(data, ct) ?? ValueTask.FromResult(WebSocketSendStatus.Closed);
    }

    /// <summary>
    /// Handles the completion of the WebSocket closure and manages connection cleanup.
    /// </summary>
    /// <param name="task">The task representing the closure operation with its result.</param>
    /// <returns>The WebSocket close result from the task.</returns>
    private WebSocketCloseResult HandleClosed(Task<WebSocketCloseResult> task)
    {
        this.Trace("start");

        if (task.Exception is not null)
            this.Error(task.Exception);

        // Guard task.Result against a faulted antecedent: rethrowing here would propagate the
        // original exception into IsClosed and silently lose the close result.
#pragma warning disable VSTHRD002
        var result = task.IsFaulted
            ? new WebSocketCloseResult(WebSocketCloseStatus.Error, task.Exception?.GetBaseException())
            : task.Result;
#pragma warning restore VSTHRD002

        lock (_locker)
        {
            var cn = Interlocked.Exchange(ref _cn, null);
            if (cn is null)
            {
                this.Trace("already not connected");
                return result;
            }

            this.Trace("start, unsubscribe from managed socket");
            cn.Managed.OnTextReceived -= HandleOnTextReceived;
            cn.Managed.OnBinaryReceived -= HandleOnBinaryReceived;
        }

        this.Trace("done");

        return result;
    }

    /// <summary>
    /// Handles text messages received from the managed WebSocket and forwards them to event subscribers.
    /// </summary>
    /// <param name="data">The received text message data.</param>
    private void HandleOnTextReceived(ReadOnlyMemory<byte> data) => OnTextReceived(data);

    /// <summary>
    /// Handles binary messages received from the managed WebSocket and forwards them to event subscribers.
    /// </summary>
    /// <param name="data">The received binary message data.</param>
    private void HandleOnBinaryReceived(ReadOnlyMemory<byte> data) => OnBinaryReceived(data);

    /// <summary>
    /// Cleans up resources when connection fails, disposing native socket and unbinding events.
    /// </summary>
    /// <param name="nativeSocket">The native WebSocket to dispose.</param>
    /// <param name="managedSocket">The managed WebSocket to unbind events from.</param>
    private void Cleanup(NativeWebSocket nativeSocket, ManagedWebSocket managedSocket)
    {
        this.Trace("start, dispose native socket");
        nativeSocket.Dispose();

        this.Trace("unbind events");
        managedSocket.OnTextReceived -= HandleOnTextReceived;
        managedSocket.OnBinaryReceived -= HandleOnBinaryReceived;

        this.Trace("done");
    }

    /// <summary>
    /// Represents a WebSocket connection pairing native and managed sockets.
    /// </summary>
    /// <param name="Native">The native WebSocket instance.</param>
    /// <param name="Managed">The managed WebSocket wrapper.</param>
    /// <param name="Logger">Logger instance for the connection.</param>
    private sealed record Connection(NativeWebSocket Native, ManagedWebSocket Managed, ILogger Logger)
        : IDisposable,
            ILogSubject
    {
        /// <summary>
        /// Disposes the connection and its native socket.
        /// </summary>
        public void Dispose()
        {
            try
            {
                this.Trace("dispose native socket");
                Native.Dispose();
            }
            catch (Exception e)
            {
                this.Trace("failed: {e}", e);
            }
        }
    }
}
