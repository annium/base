using System;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Net.Sockets;

public static class ClientSocketExtensions
{
    public static void Connect(this IClientSocket socket, Uri uri, SslClientAuthenticationOptions? authOptions = null)
    {
        uri.EnsureAbsolute();
        var entry = Dns.GetHostEntry(uri.Host).NotNull();

        var endpoint = new IPEndPoint(entry.AddressList.First(), uri.Port);
        socket.Connect(endpoint, authOptions);
    }

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
