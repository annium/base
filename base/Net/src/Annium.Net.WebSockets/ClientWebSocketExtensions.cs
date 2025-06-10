using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Net.WebSockets;

/// <summary>
/// Extension methods for client WebSocket operations
/// </summary>
public static class ClientWebSocketExtensions
{
    /// <summary>
    /// Returns a task that completes when the WebSocket is connected
    /// </summary>
    /// <param name="socket">The client WebSocket to monitor</param>
    /// <param name="ct">Cancellation token to cancel the wait operation</param>
    /// <returns>A task that completes when the WebSocket connects</returns>
    public static Task WhenConnectedAsync(this IClientWebSocket socket, CancellationToken ct = default)
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
    /// Returns a task that completes when the WebSocket is disconnected
    /// </summary>
    /// <param name="socket">The client WebSocket to monitor</param>
    /// <param name="ct">Cancellation token to cancel the wait operation</param>
    /// <returns>A task that completes when the WebSocket disconnects, returning the close status</returns>
    public static Task<WebSocketCloseStatus> WhenDisconnectedAsync(
        this IClientWebSocket socket,
        CancellationToken ct = default
    )
    {
        var tcs = new TaskCompletionSource<WebSocketCloseStatus>();

        socket.Trace<string>("subscribe {tcs} to OnConnected", tcs.GetFullId());

        void HandleDisconnected(WebSocketCloseStatus status)
        {
            socket.Trace<string>("set {tcs} to signaled state", tcs.GetFullId());
            tcs.SetResult(status);
            socket.OnDisconnected -= HandleDisconnected;
        }

        socket.OnDisconnected += HandleDisconnected;

        return tcs.Task.WaitAsync(ct);
    }
}
