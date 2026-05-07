using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Net.WebSockets;

/// <summary>
/// Interface for WebSocket components that can send messages to remote endpoints.
/// </summary>
public interface ISendingWebSocket
{
    /// <summary>
    /// Sends text message over WebSocket.
    /// </summary>
    /// <param name="text">Encoded text to be sent.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Whether sending was successful. If false is returned - either token was canceled or WebSocket exception occured. Any other exception is not caught.</returns>
    ValueTask<WebSocketSendStatus> SendTextAsync(ReadOnlyMemory<byte> text, CancellationToken ct = default);

    /// <summary>
    /// Sends binary message over WebSocket.
    /// </summary>
    /// <param name="data">Data to be sent.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Whether sending was successful. If false is returned - either token was canceled or WebSocket exception occured. Any other exception is not caught.</returns>
    ValueTask<WebSocketSendStatus> SendBinaryAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default);
}
