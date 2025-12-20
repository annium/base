using System;

namespace Annium.Net.Servers.Web;

/// <summary>
/// Provides web server helper extensions.
/// </summary>
public static class ServerExtensions
{
    /// <summary>
    /// Builds an HTTP or HTTPS URI for the specified server.
    /// </summary>
    /// <param name="server">The server hosting the web endpoint.</param>
    /// <returns>The constructed HTTP(S) URI.</returns>
    public static Uri HttpUri(this IServer server)
    {
        return new Uri($"{(server.IsSecure ? "https" : "http")}://{server.Host}:{server.Port}/");
    }

    /// <summary>
    /// Builds a WebSocket or secure WebSocket URI for the specified server.
    /// </summary>
    /// <param name="server">The server hosting the WebSocket endpoint.</param>
    /// <returns>The constructed WS(S) URI.</returns>
    public static Uri WebSocketsUri(this IServer server)
    {
        return new Uri($"{(server.IsSecure ? "wss" : "ws")}://{server.Host}:{server.Port}/");
    }
}
