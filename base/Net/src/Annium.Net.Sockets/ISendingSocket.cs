using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Net.Sockets;

/// <summary>
/// Interface for sockets that can send binary data.
/// </summary>
public interface ISendingSocket
{
    /// <summary>
    /// Sends the given message over the socket.
    /// </summary>
    /// <param name="data">The data to be sent.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>
    /// The status of the send operation: <see cref="SocketSendStatus.Ok"/> on success,
    /// <see cref="SocketSendStatus.Canceled"/> when the cancellation token signalled before or during the send,
    /// or <see cref="SocketSendStatus.Closed"/> when the underlying socket was disposed or closed by either end.
    /// </returns>
    ValueTask<SocketSendStatus> SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default);
}
