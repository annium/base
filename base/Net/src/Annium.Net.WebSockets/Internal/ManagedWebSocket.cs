using System;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Net.WebSockets.Internal;

/// <summary>
/// Internal managed WebSocket implementation that handles low-level WebSocket operations
/// </summary>
internal class ManagedWebSocket : ISendingReceivingWebSocket, ILogSubject
{
    /// <summary>
    /// Gets the logger instance for this managed WebSocket
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Default buffer size for WebSocket message receiving operations
    /// </summary>
    private const int BufferSize = 65_536;

    /// <summary>
    /// Event triggered when a text message is received
    /// </summary>
    public event Action<ReadOnlyMemory<byte>> OnTextReceived = delegate { };

    /// <summary>
    /// Event triggered when a binary message is received
    /// </summary>
    public event Action<ReadOnlyMemory<byte>> OnBinaryReceived = delegate { };

    /// <summary>
    /// The underlying native WebSocket instance
    /// </summary>
    private readonly WebSocket _socket;

    /// <summary>
    /// Initializes a new instance of the ManagedWebSocket class
    /// </summary>
    /// <param name="socket">The underlying native WebSocket instance</param>
    /// <param name="logger">Logger instance for tracing and error reporting</param>
    public ManagedWebSocket(WebSocket socket, ILogger logger)
    {
        Logger = logger;
        _socket = socket;
    }

    /// <summary>
    /// Sends a text message over the WebSocket
    /// </summary>
    /// <param name="text">The text message data to send</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The status of the send operation</returns>
    public ValueTask<WebSocketSendStatus> SendTextAsync(ReadOnlyMemory<byte> text, CancellationToken ct = default)
    {
        return SendAsync(text, WebSocketMessageType.Text, ct);
    }

    /// <summary>
    /// Sends a binary message over the WebSocket
    /// </summary>
    /// <param name="data">The binary data to send</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The status of the send operation</returns>
    public ValueTask<WebSocketSendStatus> SendBinaryAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        return SendAsync(data, WebSocketMessageType.Binary, ct);
    }

    /// <summary>
    /// Starts listening for incoming WebSocket messages until the connection is closed
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The result of the WebSocket closure</returns>
    public async Task<WebSocketCloseResult> ListenAsync(CancellationToken ct)
    {
        using var buffer = new ManagedBuffer(BufferSize);

        this.Trace("start");

        while (true)
        {
            var (isClosed, result) = await ReceiveAsync(buffer, ct);
            if (isClosed)
            {
                this.Trace(
                    result.Exception is not null
                        ? $"stop with {result.Status}: {result.Exception}"
                        : $"stop with {result.Status}"
                );
                return result;
            }
        }
    }

    /// <summary>
    /// Sends data over the WebSocket with specified message type
    /// </summary>
    /// <param name="data">The data to send</param>
    /// <param name="messageType">The type of WebSocket message (Text or Binary)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The status of the send operation</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private async ValueTask<WebSocketSendStatus> SendAsync(
        ReadOnlyMemory<byte> data,
        WebSocketMessageType messageType,
        CancellationToken ct = default
    )
    {
        try
        {
            this.Trace("{messageType} ({dataLength}) - start", messageType, data.Length);

            if (ct.IsCancellationRequested)
            {
                this.Trace("{messageType} ({dataLength}) - canceled with cancellation token", messageType, data.Length);
                return WebSocketSendStatus.Canceled;
            }

            if (_socket.State is not WebSocketState.Open)
            {
                this.Trace(
                    "{messageType} ({dataLength}) - closed because socket is not open",
                    messageType,
                    data.Length
                );
                return WebSocketSendStatus.Closed;
            }

            await _socket.SendAsync(data, messageType, true, ct).ConfigureAwait(false);
            this.Trace("{messageType} ({dataLength}) - send succeed", messageType, data.Length);

            return WebSocketSendStatus.Ok;
        }
        catch (OperationCanceledException)
        {
            this.Trace(
                "{messageType} ({dataLength}) - canceled with OperationCanceledException",
                messageType,
                data.Length
            );
            return WebSocketSendStatus.Canceled;
        }
        catch (InvalidOperationException e)
        {
            this.Trace(
                "{messageType} ({dataLength}) - closed with InvalidOperationException: {e}",
                messageType,
                data.Length,
                e
            );
            return WebSocketSendStatus.Closed;
        }
        catch (WebSocketException e)
        {
            this.Trace(
                "{messageType} ({dataLength}) - closed with WebSocketException: {e}",
                messageType,
                data.Length,
                e
            );
            return WebSocketSendStatus.Closed;
        }
        catch (SocketException e)
        {
            this.Trace("{messageType} ({dataLength}) - closed with SocketException: {e}", messageType, data.Length, e);
            return WebSocketSendStatus.Closed;
        }
        catch (Exception e)
        {
            this.Error("{messageType} ({dataLength}) - closed with Exception: {e}", messageType, data.Length, e);
            return WebSocketSendStatus.Closed;
        }
    }

    /// <summary>
    /// Receives a complete WebSocket message into the provided buffer
    /// </summary>
    /// <param name="buffer">The buffer to store received data</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A tuple indicating if the connection is closed and the close result</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private async ValueTask<(bool IsClosed, WebSocketCloseResult Result)> ReceiveAsync(
        ManagedBuffer buffer,
        CancellationToken ct
    )
    {
        // reset buffer to start writing from start
        buffer.Reset();

        while (true)
        {
            // read chunk into buffer
            var receiveResult = await ReceiveChunkAsync(buffer, ct).ConfigureAwait(false);

            // if close received - return false, indicating socket is closed
            if (receiveResult.MessageType is WebSocketMessageType.Close)
            {
                return (true, new WebSocketCloseResult(receiveResult.Status, receiveResult.Exception));
            }

            // track receiveResult count
            buffer.TrackDataSize(receiveResult.Count);

            // buffer was not big enough - grow and receive next chunk
            if (!receiveResult.EndOfMessage)
            {
                this.Trace("grow buffer");
                buffer.Grow();
                continue;
            }

            this.Trace("fire {messageType} received", receiveResult.MessageType);
            if (receiveResult.MessageType is WebSocketMessageType.Text)
                OnTextReceived(buffer.Data);
            else
                OnBinaryReceived(buffer.Data);

            return (false, new WebSocketCloseResult(WebSocketCloseStatus.ClosedRemote, null));
        }
    }

    /// <summary>
    /// Receives a chunk of data from the WebSocket into the buffer's free space
    /// </summary>
    /// <param name="buffer">The buffer to receive data into</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The result of the receive operation including message type and bytes received</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private async ValueTask<ReceiveResult> ReceiveChunkAsync(ManagedBuffer buffer, CancellationToken ct)
    {
        try
        {
            if (ct.IsCancellationRequested)
            {
                this.Trace("canceled with cancellation token");
                return new ReceiveResult(WebSocketMessageType.Close, 0, true, WebSocketCloseStatus.ClosedLocal, null);
            }

            if (_socket.State is not WebSocketState.Open)
            {
                this.Trace("closed because socket is not open");
                return new ReceiveResult(WebSocketMessageType.Close, 0, true, WebSocketCloseStatus.ClosedLocal, null);
            }

            var result = await _socket.ReceiveAsync(buffer.FreeSpace, ct).ConfigureAwait(false);
            this.Trace(
                "received {messageType} ({bytesCount} - {endOfMessage})",
                result.MessageType,
                result.Count,
                result.EndOfMessage
            );

            return new ReceiveResult(
                result.MessageType,
                result.Count,
                result.EndOfMessage,
                WebSocketCloseStatus.ClosedRemote,
                null
            );
        }
        catch (OperationCanceledException)
        {
            this.Trace("closed locally with cancellation: {isCancellationRequested}", ct.IsCancellationRequested);
            return new ReceiveResult(WebSocketMessageType.Close, 0, true, WebSocketCloseStatus.ClosedLocal, null);
        }
        catch (WebSocketException e)
        {
            this.Trace("closed remotely with WebSocketException: {e}", e);
            return new ReceiveResult(WebSocketMessageType.Close, 0, true, WebSocketCloseStatus.ClosedRemote, null);
        }
        catch (Exception e)
        {
            this.Trace("Error!!: {e}", e);
            return new ReceiveResult(WebSocketMessageType.Close, 0, true, WebSocketCloseStatus.Error, e);
        }
    }
}
