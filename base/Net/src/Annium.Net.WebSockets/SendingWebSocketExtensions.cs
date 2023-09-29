using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Net.WebSockets;

public static class SendingWebSocketExtensions
{
    public static ValueTask<WebSocketSendStatus> SendTextAsync(
        this ISendingWebSocket socket,
        string text,
        CancellationToken ct = default
    ) =>
        socket.SendTextAsync(Encoding.UTF8.GetBytes(text), ct);

    public static ValueTask<WebSocketSendStatus> SendBinaryAsync(
        this ISendingWebSocket socket,
        string text,
        CancellationToken ct = default
    ) =>
        socket.SendTextAsync(Encoding.UTF8.GetBytes(text), ct);
}