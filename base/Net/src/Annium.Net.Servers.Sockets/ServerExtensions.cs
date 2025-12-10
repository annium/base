using System;

namespace Annium.Net.Servers.Sockets;

public static class ServerExtensions
{
    public static Uri TcpUri(this IServer server)
    {
        return new Uri($"tcp://{server.Host}:{server.Port}/");
    }
}
