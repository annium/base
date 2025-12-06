using System.Net;
using Annium.Net.Servers.Sockets;

namespace Annium.Net.Sockets.Tests.Internal;

internal static class ServerExtensions
{
    public static IPEndPoint EndPoint(this IServer server)
    {
        return new IPEndPoint(IPAddress.Loopback, server.Uri.Port);
    }
}
