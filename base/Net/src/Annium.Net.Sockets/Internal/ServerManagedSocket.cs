using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Net.Sockets.Internal;

/// <summary>
/// Server-side managed socket that wraps a client connection and handles messaging
/// </summary>
internal class ServerManagedSocket : IServerManagedSocket, ILogSubject
{
    /// <summary>
    /// Logger for tracing socket operations
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Event raised when data is received from the client
    /// </summary>
    public event Action<ReadOnlyMemory<byte>> OnReceived = delegate { };

    /// <summary>
    /// Task that completes when the socket is closed
    /// </summary>
    public Task<SocketCloseResult> IsClosed { get; }

    /// <summary>
    /// The underlying stream for client communication
    /// </summary>
    private readonly Stream _stream;

    /// <summary>
    /// The managed socket wrapper for the client connection
    /// </summary>
    private readonly IManagedSocket _socket;

    /// <summary>
    /// Initializes a new instance of the ServerManagedSocket class
    /// </summary>
    /// <param name="stream">The client connection stream</param>
    /// <param name="options">Configuration options for the managed socket</param>
    /// <param name="logger">Logger for tracing socket operations</param>
    /// <param name="ct">Cancellation token for the socket lifetime</param>
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
        IsClosed = _socket.ListenAsync(ct).ContinueWith(HandleClosed);
    }

    /// <summary>
    /// Disposes the server managed socket and its underlying resources
    /// </summary>
    public void Dispose()
    {
        this.Trace("start, dispose socket");

        _socket.Dispose();

        this.Trace("done");
    }

    /// <summary>
    /// Disconnects the client socket and cleans up resources
    /// </summary>
    /// <returns>A task representing the asynchronous disconnect operation</returns>
    public async Task DisconnectAsync()
    {
        this.Trace("start");

        this.Trace("unbind events");
        _socket.OnReceived -= HandleOnReceived;

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
    /// Sends data to the connected client
    /// </summary>
    /// <param name="data">The data to send</param>
    /// <param name="ct">Cancellation token for the operation</param>
    /// <returns>The status of the send operation</returns>
    public ValueTask<SocketSendStatus> SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        this.Trace("send binary");

        return _socket.SendAsync(data, ct);
    }

    /// <summary>
    /// Handles the socket closure and cleans up event subscriptions
    /// </summary>
    /// <param name="task">The task containing the socket close result</param>
    /// <returns>The socket close result</returns>
    private SocketCloseResult HandleClosed(Task<SocketCloseResult> task)
    {
        this.Trace("start, unsubscribe from managed socket");

        if (task.Exception is not null)
            this.Error(task.Exception);

        _socket.OnReceived -= HandleOnReceived;

        this.Trace("done");

#pragma warning disable VSTHRD002
        return task.Result;
#pragma warning restore VSTHRD002
    }

    /// <summary>
    /// Handles data received from the underlying socket and forwards it to subscribers
    /// </summary>
    /// <param name="data">The received data</param>
    private void HandleOnReceived(ReadOnlyMemory<byte> data)
    {
        this.Trace("trigger binary received");
        OnReceived(data);
    }
}
