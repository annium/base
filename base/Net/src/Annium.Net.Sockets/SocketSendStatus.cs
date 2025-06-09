namespace Annium.Net.Sockets;

/// <summary>
/// Indicates the result of a socket send operation
/// </summary>
public enum SocketSendStatus
{
    /// <summary>
    /// The data was sent successfully
    /// </summary>
    Ok,

    /// <summary>
    /// The send operation was canceled
    /// </summary>
    Canceled,

    /// <summary>
    /// The socket is closed and cannot send data
    /// </summary>
    Closed,
}
