using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Net.Sockets;

public static class ServerSocketExtensions
{
    public static Task<SocketCloseStatus> WhenDisconnected(this IServerSocket socket, CancellationToken ct = default)
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