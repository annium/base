using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Net.WebSockets;

public static class ClientWebSocketExtensions
{
    public static Task WhenConnected(this IClientWebSocket socket, CancellationToken ct = default)
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

    public static Task<WebSocketCloseStatus> WhenDisconnected(this IClientWebSocket socket, CancellationToken ct = default)
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