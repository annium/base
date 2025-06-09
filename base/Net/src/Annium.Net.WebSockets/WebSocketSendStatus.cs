namespace Annium.Net.WebSockets;

/// <summary>
/// Represents the status of a WebSocket send operation
/// </summary>
public enum WebSocketSendStatus
{
    /// <summary>
    /// The message was sent successfully
    /// </summary>
    Ok,

    /// <summary>
    /// The send operation was canceled
    /// </summary>
    Canceled,

    /// <summary>
    /// The send operation failed because the WebSocket is closed
    /// </summary>
    Closed,
}
