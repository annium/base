using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Annium.Net;

/// <summary>
/// Provides extension methods for working with <see cref="IPEndPoint"/>.
/// </summary>
public static class IPEndPointExt
{
    /// <summary>
    /// The prefix used for TCP endpoints in URI format.
    /// </summary>
    private const string TcpPrefix = "tcp://";

    /// <summary>
    /// Parses a string into an <see cref="IPEndPoint"/>.
    /// </summary>
    /// <param name="s">The string to parse, e.g. "127.0.0.1:8080" or "localhost:8080".</param>
    /// <param name="defaultPort">The default port to use if none is specified in the string.</param>
    /// <returns>The parsed <see cref="IPEndPoint"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="defaultPort"/> is not a valid port number.</exception>
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

    /// <summary>
    /// Determines whether the specified port number is valid (0-65535).
    /// </summary>
    /// <param name="port">The port number to validate.</param>
    /// <returns><c>true</c> if the port is valid; otherwise, <c>false</c>.</returns>
    private static bool IsValidPort(int port) => port is >= 0 and < 65536;
}
