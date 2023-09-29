using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Net.Sockets;

public static class SendingSocketExtensions
{
    public static ValueTask<SocketSendStatus> SendTextAsync(
        this ISendingSocket socket,
        string text,
        CancellationToken ct = default
    ) =>
        socket.SendAsync(Encoding.UTF8.GetBytes(text), ct);
}