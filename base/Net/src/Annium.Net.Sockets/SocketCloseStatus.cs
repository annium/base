namespace Annium.Net.Sockets;

/// <summary>
/// Indicates how a socket connection was closed
/// </summary>
public enum SocketCloseStatus
{
    /// <summary>
    /// The connection was closed locally
    /// </summary>
    ClosedLocal,

    /// <summary>
    /// The connection was closed by the remote endpoint
    /// </summary>
    ClosedRemote,

    /// <summary>
    /// The connection was closed due to an error
    /// </summary>
    Error,
}
