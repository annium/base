using System;

namespace Annium.Net.Sockets.Internal;

/// <summary>
/// Represents the result of a socket close operation
/// </summary>
internal readonly struct SocketCloseResult
{
    /// <summary>
    /// The status indicating how the socket was closed
    /// </summary>
    public readonly SocketCloseStatus Status;

    /// <summary>
    /// The exception that caused the socket to close, if any
    /// </summary>
    public readonly Exception? Exception;

    /// <summary>
    /// Initializes a new instance of the SocketCloseResult struct
    /// </summary>
    /// <param name="status">The close status</param>
    /// <param name="exception">The exception that caused the close, if any</param>
    public SocketCloseResult(SocketCloseStatus status, Exception? exception)
    {
        Status = status;
        Exception = exception;
    }
}
