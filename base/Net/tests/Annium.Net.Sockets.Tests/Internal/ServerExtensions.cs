using System.Net;
using Annium.Net.Servers.Sockets;

namespace Annium.Net.Sockets.Tests.Internal;

/// <summary>
/// Test-only helpers for working with socket servers.
/// </summary>
internal static class ServerExtensions
{
    /// <summary>
    /// Creates a loopback endpoint from the server's advertised URI.
    /// </summary>
    /// <param name="server">Server to extract host and port information from.</param>
    /// <returns>Loopback endpoint pointing to the server instance.</returns>
    public static IPEndPoint EndPoint(this IServer server)
    {
        return new IPEndPoint(IPAddress.Loopback, server.Port);
    }
}
