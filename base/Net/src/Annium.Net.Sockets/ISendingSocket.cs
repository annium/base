using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Net.Sockets;

public interface ISendingSocket
{
    /// <summary>
    /// Sends message over Socket
    /// </summary>
    /// <param name="data">data to be sent</param>
    /// <param name="ct">cancellation token</param>
    /// <returns>Whether sending was successful. If false is returned - either token was canceled or Socket exception occured. Any other exception is not caught</returns>
    ValueTask<SocketSendStatus> SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default);
}