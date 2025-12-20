using System;

namespace Annium.Net.Servers.Sockets;

/// <summary>
/// Provides socket server helper extensions.
/// </summary>
public static class ServerExtensions
{
    /// <summary>
    /// Builds a TCP URI for the specified server.
    /// </summary>
    /// <param name="server">The server hosting the TCP endpoint.</param>
    /// <returns>The constructed TCP URI.</returns>
    public static Uri TcpUri(this IServer server)
    {
        return new Uri($"tcp://{server.Host}:{server.Port}/");
    }
}
