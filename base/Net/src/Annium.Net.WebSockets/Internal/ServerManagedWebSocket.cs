using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using NativeWebSocket = System.Net.WebSockets.WebSocket;

namespace Annium.Net.WebSockets.Internal;

/// <summary>
/// Server-side managed WebSocket implementation that wraps the native WebSocket with additional functionality
/// </summary>
internal class ServerManagedWebSocket : IServerManagedWebSocket, ILogSubject
{
    /// <summary>
    /// Gets the logger instance for this managed WebSocket
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Event triggered when a text message is received
    /// </summary>
    public event Action<ReadOnlyMemory<byte>> OnTextReceived = delegate { };

    /// <summary>
    /// Event triggered when a binary message is received
    /// </summary>
    public event Action<ReadOnlyMemory<byte>> OnBinaryReceived = delegate { };

    /// <summary>
    /// Gets a task that completes when the WebSocket is closed
    /// </summary>
    public Task<WebSocketCloseResult> IsClosed { get; }

    /// <summary>
    /// The underlying native WebSocket instance
    /// </summary>
    private readonly NativeWebSocket _nativeSocket;

    /// <summary>
    /// The managed WebSocket wrapper for the native socket
    /// </summary>
    private readonly ManagedWebSocket _managedSocket;

    /// <summary>
    /// Initializes a new instance of the ServerManagedWebSocket
    /// </summary>
    /// <param name="nativeSocket">The native WebSocket instance from the server</param>
    /// <param name="logger">Logger instance for tracing</param>
    /// <param name="ct">Cancellation token for the connection</param>
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
        IsClosed = _managedSocket.ListenAsync(ct).ContinueWith(HandleClosed);
    }

    /// <summary>
    /// Disposes the managed WebSocket and releases all resources
    /// </summary>
    public void Dispose()
    {
        this.Trace("done");
    }

    /// <summary>
    /// Disconnects the WebSocket asynchronously
    /// </summary>
    /// <returns>A task that represents the asynchronous disconnect operation</returns>
    public async Task DisconnectAsync()
    {
        this.Trace("start");

        this.Trace("unbind events");
        _managedSocket.OnTextReceived -= HandleOnTextReceived;
        _managedSocket.OnBinaryReceived -= HandleOnBinaryReceived;

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
    /// Sends a text message asynchronously
    /// </summary>
    /// <param name="text">The encoded text to send</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The status of the send operation</returns>
    public ValueTask<WebSocketSendStatus> SendTextAsync(ReadOnlyMemory<byte> text, CancellationToken ct = default)
    {
        this.Trace("send text");

        return _managedSocket.SendTextAsync(text, ct);
    }

    /// <summary>
    /// Sends binary data asynchronously
    /// </summary>
    /// <param name="data">The binary data to send</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The status of the send operation</returns>
    public ValueTask<WebSocketSendStatus> SendBinaryAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        this.Trace("send binary");

        return _managedSocket.SendBinaryAsync(data, ct);
    }

    /// <summary>
    /// Handles the completion of the WebSocket closure and manages event cleanup
    /// </summary>
    /// <param name="task">The task representing the closure operation with its result</param>
    /// <returns>The WebSocket close result from the task</returns>
    private WebSocketCloseResult HandleClosed(Task<WebSocketCloseResult> task)
    {
        this.Trace("start, unsubscribe from managed socket");

        if (task.Exception is not null)
            this.Error(task.Exception);

        _managedSocket.OnTextReceived -= HandleOnTextReceived;
        _managedSocket.OnBinaryReceived -= HandleOnBinaryReceived;

        this.Trace("done");

#pragma warning disable VSTHRD002
        return task.Result;
#pragma warning restore VSTHRD002
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
}
