namespace Annium.Net.Sockets;

/// <summary>
/// Defines the operating mode of a socket
/// </summary>
public enum SocketMode
{
    /// <summary>
    /// Raw mode - data is sent/received as-is without any framing
    /// </summary>
    Raw,

    /// <summary>
    /// Messaging mode - data is framed with length prefixes for message boundaries
    /// </summary>
    Messaging,
}
