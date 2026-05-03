using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.Sockets.Internal;

namespace Annium.Net.Sockets;

/// <summary>
/// Extension methods for server socket operations.
/// </summary>
public static class ServerSocketExtensions
{
    /// <summary>
    /// Returns a task that completes when the socket disconnects.
    /// </summary>
    /// <param name="socket">The server socket to monitor.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that completes with the disconnect status when the socket disconnects.</returns>
    public static Task<SocketCloseStatus> WhenDisconnectedAsync(
        this IServerSocket socket,
        CancellationToken ct = default
    ) =>
        SocketEventHelpers.WaitForDisconnectAsync(
            handler => socket.OnDisconnected += handler,
            handler => socket.OnDisconnected -= handler,
            socket,
            ct
        );
}
