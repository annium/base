using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Annium.Net;

// ReSharper disable once InconsistentNaming
public static class IPEndPointExt
{
    private const string TcpPrefix = "tcp://";

    public static IPEndPoint Parse(string s, int defaultPort = 0)
    {
        if (!IsValidPort(defaultPort))
            throw new ArgumentOutOfRangeException(nameof(defaultPort));

        if (!Uri.TryCreate(s.StartsWith(TcpPrefix) ? s : $"{TcpPrefix}{s}", UriKind.Absolute, out var uri))
            return new IPEndPoint(IPAddress.Loopback, defaultPort);

        var port = IsValidPort(uri.Port) ? uri.Port : defaultPort;
        if (uri.Host.Any(char.IsLetter))
            return new IPEndPoint(
                Dns.GetHostAddresses(uri.Host).First(x => x.AddressFamily == AddressFamily.InterNetwork),
                port
            );

        if (IPAddress.TryParse(uri.Host, out var ipAddress))
            return new IPEndPoint(ipAddress, port);

        return new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), port);
    }

    private static bool IsValidPort(int port) => port is >= 0 and < 65536;
}