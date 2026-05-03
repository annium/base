using System;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.Sockets.Internal;

namespace Annium.Net.Sockets;

/// <summary>
/// Extension methods for client socket operations.
/// </summary>
public static class ClientSocketExtensions
{
    /// <summary>
    /// Connects to a remote endpoint specified by URI. Resolves the host and prefers IPv4
    /// (<see cref="AddressFamily.InterNetwork"/>); falls back to IPv6 (<see cref="AddressFamily.InterNetworkV6"/>)
    /// when no IPv4 address is registered.
    /// </summary>
    /// <param name="socket">The client socket to connect.</param>
    /// <param name="uri">The URI of the remote endpoint.</param>
    /// <param name="authOptions">Optional SSL client authentication options.</param>
    public static void Connect(this IClientSocket socket, Uri uri, SslClientAuthenticationOptions? authOptions = null)
    {
        uri.EnsureAbsolute();
        var entry = Dns.GetHostEntry(uri.Host).NotNull();

        var address =
            entry.AddressList.FirstOrDefault(x => x.AddressFamily is AddressFamily.InterNetwork)
            ?? entry.AddressList.FirstOrDefault(x => x.AddressFamily is AddressFamily.InterNetworkV6)
            ?? throw new InvalidOperationException($"No IPv4 or IPv6 address found for host '{uri.Host}'.");

        var endpoint = new IPEndPoint(address, uri.Port);
        socket.Connect(endpoint, authOptions);
    }

    /// <summary>
    /// Returns a task that completes when the socket connects.
    /// </summary>
    /// <param name="socket">The client socket to monitor.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that completes when the socket connects.</returns>
    public static Task WhenConnectedAsync(this IClientSocket socket, CancellationToken ct = default)
    {
        var tcs = new TaskCompletionSource();

        socket.Trace<string>("subscribe {tcs} to OnConnected", tcs.GetFullId());

        void HandleConnected()
        {
            socket.Trace<string>("set {tcs} to signaled state", tcs.GetFullId());
            tcs.TrySetResult();
            socket.OnConnected -= HandleConnected;
        }

        socket.OnConnected += HandleConnected;

        return tcs.Task.WaitAsync(ct);
    }

    /// <summary>
    /// Returns a task that completes when the socket disconnects.
    /// </summary>
    /// <param name="socket">The client socket to monitor.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that completes with the disconnect status when the socket disconnects.</returns>
    public static Task<SocketCloseStatus> WhenDisconnectedAsync(
        this IClientSocket socket,
        CancellationToken ct = default
    ) =>
        SocketEventHelpers.WaitForDisconnectAsync(
            handler => socket.OnDisconnected += handler,
            handler => socket.OnDisconnected -= handler,
            socket,
            ct
        );
}
