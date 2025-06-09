using System;
using System.Net.WebSockets;

namespace Annium.Net.WebSockets.Internal;

/// <summary>
/// Internal structure representing the result of a WebSocket receive operation
/// </summary>
internal readonly struct ReceiveResult
{
    /// <summary>
    /// The type of the received WebSocket message
    /// </summary>
    public readonly WebSocketMessageType MessageType;

    /// <summary>
    /// The number of bytes received
    /// </summary>
    public readonly int Count;

    /// <summary>
    /// Indicates whether this is the end of the message
    /// </summary>
    public readonly bool EndOfMessage;

    /// <summary>
    /// The close status if the message type is Close
    /// </summary>
    public readonly WebSocketCloseStatus Status;

    /// <summary>
    /// The exception that occurred during receive, if any
    /// </summary>
    public readonly Exception? Exception;

    /// <summary>
    /// Initializes a new instance of the ReceiveResult structure
    /// </summary>
    /// <param name="messageType">The type of the received message</param>
    /// <param name="count">The number of bytes received</param>
    /// <param name="endOfMessage">Whether this is the end of the message</param>
    /// <param name="status">The close status if applicable</param>
    /// <param name="exception">The exception that occurred, if any</param>
    public ReceiveResult(
        WebSocketMessageType messageType,
        int count,
        bool endOfMessage,
        WebSocketCloseStatus status,
        Exception? exception
    )
    {
        MessageType = messageType;
        Count = count;
        EndOfMessage = endOfMessage;
        Status = status;
        Exception = exception;
    }
}
