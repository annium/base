using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Net.Sockets.Internal;

/// <summary>
/// Internal implementation of a client-side managed socket that handles connection, SSL, and data transmission.
/// </summary>
internal class ClientManagedSocket : IClientManagedSocket, ILogSubject
{
    /// <summary>
    /// Gets the logger instance.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Event raised when data is received from the remote endpoint.
    /// </summary>
    public event Action<ReadOnlyMemory<byte>> OnReceived = delegate { };

    /// <summary>
    /// Gets a task that completes when the socket is closed.
    /// </summary>
    public Task<SocketCloseResult> IsClosed { get; private set; } =
        Task.FromResult(new SocketCloseResult(SocketCloseStatus.ClosedLocal, null));

    /// <summary>
    /// Configuration options for the managed socket.
    /// </summary>
    private readonly ManagedSocketOptions _options;

    /// <summary>
    /// Thread synchronization lock for connection operations.
    /// </summary>
    private readonly Lock _locker = new();

    /// <summary>
    /// Current connection state including stream and cancellation token.
    /// </summary>
    private Connection? _cn;

    /// <summary>
    /// Cancellation token source for listening operations.
    /// </summary>
    private CancellationTokenSource _listenCts = new();

    /// <summary>
    /// Initializes a new instance of the ClientManagedSocket class.
    /// </summary>
    /// <param name="options">Configuration options for the socket.</param>
    /// <param name="logger">Logger instance for diagnostics.</param>
    public ClientManagedSocket(ManagedSocketOptions options, ILogger logger)
    {
        _options = options;
        Logger = logger;
    }

    /// <summary>
    /// Disposes the socket and releases all resources.
    /// </summary>
    public void Dispose()
    {
        this.Trace("start");

        TeardownUnderLock();

        // TeardownUnderLock only disposes _listenCts when a live connection was torn down; on the
        // never-connected (or double-dispose) path it returns early. Dispose _listenCts here so the
        // constructor-created instance is always released. CTS.Dispose() is idempotent, so the
        // connected path (already disposed under the lock) is a safe no-op.
#pragma warning disable VSTHRD103
        _listenCts.Dispose();
#pragma warning restore VSTHRD103

        this.Trace("done");
    }

    /// <summary>
    /// Connects to the specified remote endpoint asynchronously.
    /// </summary>
    /// <param name="endpoint">The remote endpoint to connect to.</param>
    /// <param name="authOptions">Optional SSL client authentication options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that completes with null on success or an exception on failure.</returns>
    public async Task<Exception?> ConnectAsync(
        IPEndPoint endpoint,
        SslClientAuthenticationOptions? authOptions,
        CancellationToken ct = default
    )
    {
        this.Trace("start");

        // only connection is checked, because after disconnect listen task can still be awaited
        if (_cn is not null)
            throw new InvalidOperationException("Socket is already connected");

        Stream? stream = null;
        IManagedSocket? socket = null;
        Socket? nativeSocket = null;
        try
        {
            this.Trace("create native socket");
            nativeSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);

            this.Trace("connect native socket to {endpoint}", endpoint);
            await nativeSocket.ConnectAsync(endpoint, ct);

            if (authOptions is not null)
            {
                this.Trace("wrap native socket with network stream");
                var networkStream = new NetworkStream(nativeSocket, true);

                this.Trace("wrap network stream with ssl stream");
                var sslStream = new SslStream(
                    networkStream,
                    false,
                    authOptions.RemoteCertificateValidationCallback,
                    null
                );
                stream = sslStream;

                this.Trace("authenticate client");
                await sslStream.AuthenticateAsClientAsync(authOptions, ct);
            }
            else
            {
                this.Trace("wrap native socket with network stream");
                stream = new NetworkStream(nativeSocket, true);
            }

            this.Trace("wrap to managed socket");
            socket = Helper.GetManagedSocket(stream, _options, Logger);
            this.Trace<string, string>(
                "paired with {nativeSocket} / {managedSocket}",
                stream.GetFullId(),
                socket.GetFullId()
            );

            this.Trace("bind events");
            socket.OnReceived += HandleOnReceived;

            var cn = new Connection(stream, socket, Logger);

            lock (_locker)
            {
                if (ct.IsCancellationRequested)
                {
                    this.Trace("connection canceled, dispose");
#pragma warning disable VSTHRD103
                    cn.Dispose();
#pragma warning restore VSTHRD103

                    // return a non-null exception, not null: null is the success sentinel, which would
                    // make the caller fire OnConnected and subscribe the stale pre-connect IsClosed task
                    // even though the connection was just torn down. A non-null result routes the caller
                    // through its failed-connect path instead.
                    return new OperationCanceledException(ct);
                }

                this.Trace("save connection");
                _cn = cn;

                this.Trace("create listen cts");
                // dispose the previous CTS before replacing it: the constructor-created instance
                // is never used for a listen task and would otherwise leak on first connect;
                // CancellationTokenSource.Dispose() is idempotent, so a reconnect (where teardown
                // already disposed it) is safe.
                // VSTHRD103: CancellationTokenSource.Dispose() is synchronous (no DisposeAsync).
#pragma warning disable VSTHRD103
                _listenCts.Dispose();
#pragma warning restore VSTHRD103
                _listenCts = new CancellationTokenSource();

                this.Trace("create listen task");
                IsClosed = socket
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
            this.Error("failed: {e}", e);

            Cleanup(stream, socket);

            // if no Stream wrapped the native socket yet (e.g. nativeSocket.ConnectAsync threw on
            // timeout/refusal), the NetworkStream never took ownership (ownsSocket: true) — dispose
            // the native socket here to avoid leaking the OS handle on every failed connect.
#pragma warning disable VSTHRD103
            if (stream is null)
                nativeSocket?.Dispose();
#pragma warning restore VSTHRD103

            this.Trace("done (not connected)");

            return e;
        }
    }

    /// <summary>
    /// Disconnects from the remote endpoint asynchronously.
    /// </summary>
    /// <returns>A task that completes when disconnection is finished.</returns>
    public async Task DisconnectAsync()
    {
        this.Trace("start");

        if (!TeardownUnderLock())
            return;

        this.Trace("await listen task");
#pragma warning disable VSTHRD003
        await IsClosed;
#pragma warning restore VSTHRD003

        this.Trace("done");
    }

    /// <summary>
    /// Performs the synchronous teardown shared by <see cref="Dispose"/> and <see cref="DisconnectAsync"/>:
    /// claim the live connection under <c>_locker</c>, unbind events, cancel + dispose the listen CTS, and
    /// dispose the connection.
    /// </summary>
    /// <returns>True if a live connection was torn down, false if the socket was not connected.</returns>
    private bool TeardownUnderLock()
    {
        lock (_locker)
        {
            var cn = Interlocked.Exchange(ref _cn, null);
            if (cn is null)
            {
                this.Trace("skip - not connected");
                return false;
            }

            this.Trace("unbind events");
            cn.Socket.OnReceived -= HandleOnReceived;

            this.Trace("cancel listen cts");
#pragma warning disable VSTHRD103
            _listenCts.Cancel();
            _listenCts.Dispose();

            this.Trace("dispose connection");
            cn.Dispose();
#pragma warning restore VSTHRD103
            return true;
        }
    }

    /// <summary>
    /// Sends data to the remote endpoint asynchronously.
    /// </summary>
    /// <param name="data">The data to send.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The status of the send operation.</returns>
    public ValueTask<SocketSendStatus> SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        this.Trace("send");

        return _cn?.Socket.SendAsync(data, ct) ?? ValueTask.FromResult(SocketSendStatus.Closed);
    }

    /// <summary>
    /// Handles when the underlying socket is closed.
    /// </summary>
    /// <param name="task">The socket close task result.</param>
    /// <returns>The socket close result.</returns>
    private SocketCloseResult HandleClosed(Task<SocketCloseResult> task)
    {
        this.Trace("start");

        if (task.Exception is not null)
            this.Error(task.Exception);

        // Guard task.Result against a faulted antecedent (mirror of ServerManagedSocket.HandleClosed):
        // accessing task.Result on a faulted Task throws AggregateException, which would propagate
        // into IsClosed and silently lose the close result.
        if (task.IsFaulted)
        {
            var faultedResult = new SocketCloseResult(SocketCloseStatus.Error, task.Exception?.GetBaseException());

            lock (_locker)
            {
                var cn = Interlocked.Exchange(ref _cn, null);
                if (cn is null)
                {
                    this.Trace("skip - not connected (faulted)");
                    return faultedResult;
                }

                this.Trace("unbind events (faulted)");
                cn.Socket.OnReceived -= HandleOnReceived;

                this.Trace("dispose connection (faulted)");
                cn.Dispose();
            }

            this.Trace("done (faulted)");
            return faultedResult;
        }

#pragma warning disable VSTHRD002
        lock (_locker)
        {
            var cn = Interlocked.Exchange(ref _cn, null);
            if (cn is null)
            {
                this.Trace("skip - not connected");
                return task.Result;
            }

            this.Trace("unbind events");
            cn.Socket.OnReceived -= HandleOnReceived;

            this.Trace("dispose connection");
            cn.Dispose();
        }

        this.Trace("done");

        return task.Result;
#pragma warning restore VSTHRD002
    }

    /// <summary>
    /// Handles received data from the underlying socket.
    /// </summary>
    /// <param name="data">The received data.</param>
    private void HandleOnReceived(ReadOnlyMemory<byte> data)
    {
        this.Trace("trigger binary received");
        OnReceived(data);
    }

    /// <summary>
    /// Cleans up resources when connection fails.
    /// </summary>
    /// <param name="stream">The stream to dispose.</param>
    /// <param name="socket">The socket to dispose.</param>
    private void Cleanup(Stream? stream, IManagedSocket? socket)
    {
        this.Trace("start");

        if (stream is not null)
        {
            this.Trace("dispose native socket");
            stream.Close();
        }

        if (socket is not null)
        {
            this.Trace("unbind events and dispose socket");
            socket.OnReceived -= HandleOnReceived;
            socket.Dispose();
        }

        this.Trace("done");
    }

    /// <summary>
    /// Represents a connection with its stream and socket.
    /// </summary>
    /// <param name="Stream">The network stream.</param>
    /// <param name="Socket">The managed socket.</param>
    /// <param name="Logger">The logger instance.</param>
    private sealed record Connection(Stream Stream, IManagedSocket Socket, ILogger Logger) : IDisposable, ILogSubject
    {
        /// <summary>
        /// Disposes the connection resources.
        /// </summary>
        public void Dispose()
        {
            try
            {
                this.Trace("dispose socket");
                Socket.Dispose();

                this.Trace("close stream");
                Stream.Close();
            }
            catch (Exception e)
            {
                this.Trace("failed: {e}", e);
            }
        }
    }
}
