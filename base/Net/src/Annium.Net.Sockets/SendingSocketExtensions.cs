using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Net.Sockets;

/// <summary>
/// Extension methods for sending socket operations
/// </summary>
public static class SendingSocketExtensions
{
    /// <summary>
    /// Sends text data over the socket using UTF-8 encoding
    /// </summary>
    /// <param name="socket">The sending socket</param>
    /// <param name="text">The text to send</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The status of the send operation</returns>
    public static ValueTask<SocketSendStatus> SendTextAsync(
        this ISendingSocket socket,
        string text,
        CancellationToken ct = default
    ) => socket.SendAsync(Encoding.UTF8.GetBytes(text), ct);
}
