using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Net.WebSockets;

public static class ServerWebSocketExtensions
{
    public static Task<WebSocketCloseStatus> WhenDisconnected(this IServerWebSocket socket, CancellationToken ct = default)
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