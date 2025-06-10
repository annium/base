using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Net.Sockets;

/// <summary>
/// Extension methods for server socket operations
/// </summary>
public static class ServerSocketExtensions
{
    /// <summary>
    /// Returns a task that completes when the socket disconnects
    /// </summary>
    /// <param name="socket">The server socket to monitor</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A task that completes with the disconnect status when the socket disconnects</returns>
    public static Task<SocketCloseStatus> WhenDisconnectedAsync(
        this IServerSocket socket,
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
