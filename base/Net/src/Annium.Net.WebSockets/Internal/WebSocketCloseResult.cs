using System;

namespace Annium.Net.WebSockets.Internal;

/// <summary>
/// Internal structure representing the result of a WebSocket close operation
/// </summary>
internal readonly struct WebSocketCloseResult
{
    /// <summary>
    /// The status of the WebSocket close operation
    /// </summary>
    public readonly WebSocketCloseStatus Status;

    /// <summary>
    /// The exception that caused the close, if any
    /// </summary>
    public readonly Exception? Exception;

    /// <summary>
    /// Initializes a new instance of the WebSocketCloseResult structure
    /// </summary>
    /// <param name="status">The close status</param>
    /// <param name="exception">The exception that caused the close, if any</param>
    public WebSocketCloseResult(WebSocketCloseStatus status, Exception? exception)
    {
        Status = status;
        Exception = exception;
    }
}
