using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Net.Sockets;

public static class ClientSocketExtensions
{
    public static Task WhenConnected(this IClientSocket socket, CancellationToken ct = default)
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

    public static Task<SocketCloseStatus> WhenDisconnected(this IClientSocket socket, CancellationToken ct = default)
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