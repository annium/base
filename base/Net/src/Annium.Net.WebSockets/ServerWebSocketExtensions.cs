using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.WebSockets.Internal;

namespace Annium.Net.WebSockets;

/// <summary>
/// Extension methods for server WebSocket operations.
/// </summary>
public static class ServerWebSocketExtensions
{
    /// <summary>
    /// Returns a task that completes when the WebSocket is disconnected.
    /// </summary>
    /// <param name="socket">The server WebSocket to monitor.</param>
    /// <param name="ct">Cancellation token to cancel the wait operation.</param>
    /// <returns>A task that completes when the WebSocket disconnects, returning the close status.</returns>
    public static Task<WebSocketCloseStatus> WhenDisconnectedAsync(
        this IServerWebSocket socket,
        CancellationToken ct = default
    ) =>
        WebSocketEventHelpers.WaitForDisconnectAsync(
            handler => socket.OnDisconnected += handler,
            handler => socket.OnDisconnected -= handler,
            socket,
            ct
        );
}
