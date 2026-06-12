using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using NativeWebSocket = System.Net.WebSockets.WebSocket;

namespace Annium.Net.WebSockets.Internal;

/// <summary>
/// Server-side managed WebSocket implementation that wraps the native WebSocket with additional functionality.
/// </summary>
internal class ServerManagedWebSocket : IServerManagedWebSocket, ILogSubject
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
    public Task<WebSocketCloseResult> IsClosed { get; }

    /// <summary>
    /// The underlying native WebSocket instance.
    /// </summary>
    private readonly NativeWebSocket _nativeSocket;

    /// <summary>
    /// The managed WebSocket wrapper for the native socket.
    /// </summary>
    private readonly ManagedWebSocket _managedSocket;

    /// <summary>
    /// Once-only teardown guard (1 = teardown ran). Set via <see cref="Interlocked"/>.CompareExchange
    /// so the unbind/close sequence runs at most once across <see cref="Dispose"/>,
    /// <see cref="DisconnectAsync"/>, and <see cref="HandleClosed"/>.
    /// </summary>
    private int _tornDown;

    /// <summary>
    /// Initializes a new instance of the ServerManagedWebSocket.
    /// </summary>
    /// <param name="nativeSocket">The native WebSocket instance from the server.</param>
    /// <param name="logger">Logger instance for tracing.</param>
    /// <param name="ct">Cancellation token for the connection.</param>
    public ServerManagedWebSocket(NativeWebSocket nativeSocket, ILogger logger, CancellationToken ct = default)
    {
        Logger = logger;
        _nativeSocket = nativeSocket;
        _managedSocket = new ManagedWebSocket(nativeSocket, logger);
        this.Trace<string, string>(
            "paired with {nativeSocket} / {managedSocket}",
            _nativeSocket.GetFullId(),
            _managedSocket.GetFullId()
        );

        _managedSocket.OnTextReceived += HandleOnTextReceived;
        _managedSocket.OnBinaryReceived += HandleOnBinaryReceived;

        this.Trace("start listen");
        IsClosed = _managedSocket
            .ListenAsync(ct)
            .ContinueWith(
                HandleClosed,
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default
            );
    }

    /// <summary>
    /// Disposes the managed WebSocket and releases all resources.
    /// </summary>
    public void Dispose()
    {
        this.Trace("start");

        // Gate abort + unbind on the once-only teardown claim. Native disposal is
        // unconditional (and idempotent per WebSocket.Dispose semantics): if a winner
        // already ran (HandleClosed or DisconnectAsync), the native socket has not been
        // disposed yet — those paths only unbind and (for DisconnectAsync) send the
        // close-output frame. Dispose is the sole owner of native disposal.
        if (TryClaimTeardown())
        {
            UnbindEvents();

            try
            {
                // synchronous abort; Dispose cannot await CloseOutputAsync
                if (_nativeSocket.State is WebSocketState.Open or WebSocketState.CloseReceived)
                    _nativeSocket.Abort();
            }
            catch (Exception e)
            {
                this.Trace("abort failed: {e}", e);
            }
        }
        else
        {
            this.Trace("teardown already claimed; skip abort + unbind");
        }

        _nativeSocket.Dispose();

        this.Trace("done");
    }

    /// <summary>
    /// Disconnects the WebSocket asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous disconnect operation.</returns>
    public async Task DisconnectAsync()
    {
        this.Trace("start");

        if (!TryClaimTeardown())
        {
            this.Trace("skip - already torn down");
            return;
        }

        this.Trace("unbind events");
        UnbindEvents();

        try
        {
            this.Trace("close output");
            if (_nativeSocket.State is WebSocketState.Open or WebSocketState.CloseReceived)
                await _nativeSocket.CloseOutputAsync(
                    System.Net.WebSockets.WebSocketCloseStatus.NormalClosure,
                    string.Empty,
                    CancellationToken.None
                );
        }
        catch (Exception e)
        {
            this.Trace("failed: {e}", e);
        }

        this.Trace("done");
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

        return _managedSocket.SendTextAsync(text, ct);
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

        return _managedSocket.SendBinaryAsync(data, ct);
    }

    /// <summary>
    /// Handles the completion of the WebSocket closure and manages event cleanup.
    /// </summary>
    /// <param name="task">The task representing the closure operation with its result.</param>
    /// <returns>The WebSocket close result from the task.</returns>
    private WebSocketCloseResult HandleClosed(Task<WebSocketCloseResult> task)
    {
        this.Trace("start, unsubscribe from managed socket");

        if (task.Exception is not null)
            this.Error(task.Exception);

        if (TryClaimTeardown())
        {
            UnbindEvents();
        }
        else
        {
            this.Trace("skip - already torn down");
        }

        this.Trace("done");

        // Guard task.Result against a faulted antecedent: rethrowing here would propagate the
        // original exception into IsClosed and silently lose the close result.
        if (task.IsFaulted)
        {
            return new WebSocketCloseResult(WebSocketCloseStatus.Error, task.Exception?.GetBaseException());
        }

        // VSTHRD002: task.Result is safe — the antecedent is completed and the faulted case is handled above.
#pragma warning disable VSTHRD002
        return task.Result;
#pragma warning restore VSTHRD002
    }

    /// <summary>
    /// Atomically claims the once-only teardown right. Returns true for the first caller and
    /// false for every subsequent caller (Dispose/DisconnectAsync/HandleClosed all race for it).
    /// </summary>
    /// <returns>True if this caller won the race and should perform teardown.</returns>
    private bool TryClaimTeardown() => Interlocked.CompareExchange(ref _tornDown, 1, 0) == 0;

    /// <summary>
    /// Unsubscribes the trampoline handlers from the managed socket. Idempotent on its own,
    /// but only called by the teardown winner so the unsubscribe runs exactly once.
    /// </summary>
    private void UnbindEvents()
    {
        _managedSocket.OnTextReceived -= HandleOnTextReceived;
        _managedSocket.OnBinaryReceived -= HandleOnBinaryReceived;
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
}
