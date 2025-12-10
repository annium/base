using System;

namespace Annium.Net.Servers.Web;

public static class ServerExtensions
{
    public static Uri HttpUri(this IServer server)
    {
        return new Uri($"{(server.IsSecure ? "https" : "http")}://{server.Host}:{server.Port}/");
    }

    public static Uri WebSocketsUri(this IServer server)
    {
        return new Uri($"{(server.IsSecure ? "wss" : "ws")}://{server.Host}:{server.Port}/");
    }
}
