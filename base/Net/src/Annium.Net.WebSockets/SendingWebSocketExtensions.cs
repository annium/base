using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Net.WebSockets;

/// <summary>
/// Extension methods for sending WebSocket operations with string convenience methods
/// </summary>
public static class SendingWebSocketExtensions
{
    /// <summary>
    /// Sends a text message over the WebSocket using UTF-8 encoding
    /// </summary>
    /// <param name="socket">The sending WebSocket</param>
    /// <param name="text">The text message to send</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The status of the send operation</returns>
    public static ValueTask<WebSocketSendStatus> SendTextAsync(
        this ISendingWebSocket socket,
        string text,
        CancellationToken ct = default
    ) => socket.SendTextAsync(Encoding.UTF8.GetBytes(text), ct);

    /// <summary>
    /// Sends a binary message over the WebSocket using UTF-8 encoded string data
    /// </summary>
    /// <param name="socket">The sending WebSocket</param>
    /// <param name="text">The text to send as binary data</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The status of the send operation</returns>
    public static ValueTask<WebSocketSendStatus> SendBinaryAsync(
        this ISendingWebSocket socket,
        string text,
        CancellationToken ct = default
    ) => socket.SendBinaryAsync(Encoding.UTF8.GetBytes(text), ct);
}
