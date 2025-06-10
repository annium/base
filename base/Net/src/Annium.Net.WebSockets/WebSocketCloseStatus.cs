namespace Annium.Net.WebSockets;

/// <summary>
/// Represents the status of a WebSocket connection closure
/// </summary>
public enum WebSocketCloseStatus
{
    /// <summary>
    /// The WebSocket was closed by the local endpoint
    /// </summary>
    ClosedLocal,

    /// <summary>
    /// The WebSocket was closed by the remote endpoint
    /// </summary>
    ClosedRemote,

    /// <summary>
    /// The WebSocket was closed due to an error
    /// </summary>
    Error,
}
