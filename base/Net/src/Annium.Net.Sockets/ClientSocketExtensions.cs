using System;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Net.Sockets;

/// <summary>
/// Extension methods for client socket operations
/// </summary>
public static class ClientSocketExtensions
{
    /// <summary>
    /// Connects to a remote endpoint specified by URI
    /// </summary>
    /// <param name="socket">The client socket to connect</param>
    /// <param name="uri">The URI of the remote endpoint</param>
    /// <param name="authOptions">Optional SSL client authentication options</param>
    public static void Connect(this IClientSocket socket, Uri uri, SslClientAuthenticationOptions? authOptions = null)
    {
        uri.EnsureAbsolute();
        var entry = Dns.GetHostEntry(uri.Host).NotNull();

        var endpoint = new IPEndPoint(
            entry.AddressList.First(x => x.AddressFamily is AddressFamily.InterNetwork),
            uri.Port
        );
        socket.Connect(endpoint, authOptions);
    }

    /// <summary>
    /// Returns a task that completes when the socket connects
    /// </summary>
    /// <param name="socket">The client socket to monitor</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A task that completes when the socket connects</returns>
    public static Task WhenConnectedAsync(this IClientSocket socket, CancellationToken ct = default)
    {
        var tcs = new TaskCompletionSource();

        socket.Trace<string>("subscribe {tcs} to OnConnected", tcs.GetFullId());

        void HandleConnected()
        {
            socket.Trace<string>("set {tcs} to signaled state", tcs.GetFullId());
            tcs.SetResult();
            socket.OnConnected -= HandleConnected;
        }

        socket.OnConnected += HandleConnected;

        return tcs.Task.WaitAsync(ct);
    }

    /// <summary>
    /// Returns a task that completes when the socket disconnects
    /// </summary>
    /// <param name="socket">The client socket to monitor</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A task that completes with the disconnect status when the socket disconnects</returns>
    public static Task<SocketCloseStatus> WhenDisconnectedAsync(
        this IClientSocket socket,
        CancellationToken ct = default
    )
    {
        var tcs = new TaskCompletionSource<SocketCloseStatus>();

        socket.Trace<string>("subscribe {tcs} to OnConnected", tcs.GetFullId());

        void HandleDisconnected(SocketCloseStatus status)
        {
            socket.Trace<string>("set {tcs} to signaled state", tcs.GetFullId());
            tcs.SetResult(status);
            socket.OnDisconnected -= HandleDisconnected;
        }

        socket.OnDisconnected += HandleDisconnected;

        return tcs.Task.WaitAsync(ct);
    }
}
