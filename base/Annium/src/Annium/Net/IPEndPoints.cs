using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Net;

/// <summary>
/// Provides static factory methods for working with <see cref="IPEndPoint"/>.
/// </summary>
public static class IpEndPoints
{
    /// <summary>
    /// The prefix used for TCP endpoints in URI format.
    /// </summary>
    private const string TcpPrefix = "tcp://";

    /// <summary>
    /// Parses a string into an <see cref="IPEndPoint"/>, resolving hostnames via async DNS lookup.
    /// </summary>
    /// <param name="s">The string to parse, e.g. "127.0.0.1:8080" or "localhost:8080".</param>
    /// <param name="defaultPort">The default port to use if none is specified in the string.</param>
    /// <param name="ct">The cancellation token to observe.</param>
    /// <returns>The parsed <see cref="IPEndPoint"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="defaultPort"/> is not a valid port number.</exception>
    /// <exception cref="ArgumentException">Thrown if the hostname cannot be resolved to an IPv4 address.</exception>
    public static async Task<IPEndPoint> ParseAsync(string s, int defaultPort = 0, CancellationToken ct = default)
    {
        if (!IsValidPort(defaultPort))
            throw new ArgumentOutOfRangeException(nameof(defaultPort));

        if (!Uri.TryCreate(s.StartsWith(TcpPrefix) ? s : $"{TcpPrefix}{s}", UriKind.Absolute, out var uri))
            return new IPEndPoint(IPAddress.Loopback, defaultPort);

        var port = IsValidPort(uri.Port) ? uri.Port : defaultPort;
        if (uri.Host.Any(char.IsLetter))
        {
            var addresses = await Dns.GetHostAddressesAsync(uri.Host, ct);
            var ipv4 = addresses.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
            if (ipv4 is null)
                throw new ArgumentException($"Could not resolve an IPv4 address for {uri.Host}", nameof(s));
            return new IPEndPoint(ipv4, port);
        }

        if (IPAddress.TryParse(uri.Host, out var ipAddress))
            return new IPEndPoint(ipAddress, port);

        return new IPEndPoint(new IPAddress([127, 0, 0, 1]), port);
    }

    /// <summary>
    /// Determines whether the specified port number is valid (0-65535).
    /// </summary>
    /// <param name="port">The port number to validate.</param>
    /// <returns><c>true</c> if the port is valid; otherwise, <c>false</c>.</returns>
    private static bool IsValidPort(int port) => port is >= 0 and < 65536;
}
