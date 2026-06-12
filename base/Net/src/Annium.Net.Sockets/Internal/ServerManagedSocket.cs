using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Net.Sockets.Internal;

/// <summary>
/// Server-side managed socket that wraps a client connection and handles messaging.
/// </summary>
internal class ServerManagedSocket : IServerManagedSocket, ILogSubject
{
    /// <summary>
    /// Logger for tracing socket operations.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Event raised when data is received from the client.
    /// </summary>
    public event Action<ReadOnlyMemory<byte>> OnReceived = delegate { };

    /// <summary>
    /// Task that completes when the socket is closed.
    /// </summary>
    public Task<SocketCloseResult> IsClosed { get; }

    /// <summary>
    /// The underlying stream for client communication.
    /// </summary>
    private readonly Stream _stream;

    /// <summary>
    /// The managed socket wrapper for the client connection.
    /// </summary>
    private readonly IManagedSocket _socket;

    /// <summary>
    /// Once-only teardown guard (1 = teardown ran). Set via <see cref="Interlocked"/>.CompareExchange
    /// so the receive-trampoline unbind runs at most once across <see cref="Dispose"/>,
    /// <see cref="DisconnectAsync"/>, and <see cref="HandleClosed"/> — preventing a late
    /// <see cref="HandleClosed"/> from racing a completed <see cref="Dispose"/> on the event handler.
    /// </summary>
    private int _tornDown;

    /// <summary>
    /// Initializes a new instance of the ServerManagedSocket class.
    /// </summary>
    /// <param name="stream">The client connection stream.</param>
    /// <param name="options">Configuration options for the managed socket.</param>
    /// <param name="logger">Logger for tracing socket operations.</param>
    /// <param name="ct">Cancellation token for the socket lifetime.</param>
    public ServerManagedSocket(Stream stream, ManagedSocketOptions options, ILogger logger, CancellationToken ct)
    {
        Logger = logger;
        _stream = stream;
        _socket = Helper.GetManagedSocket(stream, options, logger);
        this.Trace<string, string>(
            "paired with {nativeSocket} / {managedSocket}",
            _stream.GetFullId(),
            _socket.GetFullId()
        );

        _socket.OnReceived += HandleOnReceived;

        this.Trace("start listen");
        IsClosed = _socket
            .ListenAsync(ct)
            .ContinueWith(
                HandleClosed,
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default
            );
    }

    /// <summary>
    /// Disposes the server managed socket with forced-close semantics: unbinds the receive
    /// trampoline, disposes the inner managed socket, and closes the stream. Idempotent: a
    /// follow-up <see cref="DisconnectAsync"/> is safe (the event unsubscribe is gated by the
    /// once-only teardown claim; <see cref="Stream.Close"/> is idempotent; the inner socket's
    /// <see cref="IDisposable.Dispose"/> is idempotent).
    /// </summary>
    public void Dispose()
    {
        this.Trace("start");

        if (TryClaimTeardown())
            UnbindEvents();

        try
        {
            _socket.Dispose();
        }
        catch (Exception e)
        {
            this.Trace("inner socket dispose failed: {e}", e);
        }

        try
        {
            _stream.Close();
        }
        catch (Exception e)
        {
            this.Trace("stream close failed: {e}", e);
        }

        this.Trace("done");
    }

    /// <summary>
    /// Disconnects the client socket and cleans up resources.
    /// </summary>
    /// <returns>A task representing the asynchronous disconnect operation.</returns>
    public async Task DisconnectAsync()
    {
        this.Trace("start");

        if (TryClaimTeardown())
        {
            this.Trace("unbind events");
            UnbindEvents();
        }

        try
        {
            this.Trace("dispose socket");
            await _socket.DisposeAsync();

            this.Trace("close stream");
            _stream.Close();
        }
        catch (Exception e)
        {
            this.Trace("failed: {e}", e);
        }

        this.Trace("done");
    }

    /// <summary>
    /// Sends data to the connected client.
    /// </summary>
    /// <param name="data">The data to send.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>The status of the send operation.</returns>
    public ValueTask<SocketSendStatus> SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        this.Trace("send binary");

        return _socket.SendAsync(data, ct);
    }

    /// <summary>
    /// Handles the socket closure and cleans up event subscriptions.
    /// </summary>
    /// <param name="task">The task containing the socket close result.</param>
    /// <returns>The socket close result.</returns>
    private SocketCloseResult HandleClosed(Task<SocketCloseResult> task)
    {
        this.Trace("start, unsubscribe from managed socket");

        if (task.Exception is not null)
            this.Error(task.Exception);

        if (TryClaimTeardown())
            UnbindEvents();

        this.Trace("done");

        // Guard task.Result against a faulted antecedent: rethrowing here would propagate the
        // original exception into the IsClosed task and silently lose the close result. The
        // sibling ClientManagedSocket.HandleClosed has the same shape and benefits from the
        // same guard.
        if (task.IsFaulted)
        {
            return new SocketCloseResult(SocketCloseStatus.Error, task.Exception?.GetBaseException());
        }

#pragma warning disable VSTHRD002
        return task.Result;
#pragma warning restore VSTHRD002
    }

    /// <summary>
    /// Handles data received from the underlying socket and forwards it to subscribers.
    /// </summary>
    /// <param name="data">The received data.</param>
    private void HandleOnReceived(ReadOnlyMemory<byte> data)
    {
        this.Trace("trigger binary received");
        OnReceived(data);
    }

    /// <summary>
    /// Atomically claims the once-only teardown right. Returns true for the first caller and
    /// false for every subsequent caller (Dispose/DisconnectAsync/HandleClosed all race for it).
    /// </summary>
    /// <returns>True if this caller won the race and should unbind the receive trampoline.</returns>
    private bool TryClaimTeardown() => Interlocked.CompareExchange(ref _tornDown, 1, 0) == 0;

    /// <summary>
    /// Unsubscribes the receive trampoline from the managed socket. Only called by the teardown
    /// winner so the unsubscribe runs exactly once.
    /// </summary>
    private void UnbindEvents()
    {
        _socket.OnReceived -= HandleOnReceived;
    }
}
