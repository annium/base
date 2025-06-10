using System;

namespace Annium.Net.Sockets.Internal;

/// <summary>
/// Represents the result of a socket receive operation
/// </summary>
internal readonly struct ReceiveResult
{
    /// <summary>
    /// The number of bytes received
    /// </summary>
    public readonly int Count;

    /// <summary>
    /// The socket close status if the socket was closed during receive
    /// </summary>
    public readonly SocketCloseStatus? Status;

    /// <summary>
    /// The exception that occurred during receive, if any
    /// </summary>
    public readonly Exception? Exception;

    /// <summary>
    /// Initializes a new instance of the ReceiveResult struct
    /// </summary>
    /// <param name="count">The number of bytes received</param>
    /// <param name="status">The socket close status, if applicable</param>
    /// <param name="exception">The exception that occurred, if any</param>
    public ReceiveResult(int count, SocketCloseStatus? status, Exception? exception)
    {
        Count = count;
        Status = status;
        Exception = exception;
    }
}
