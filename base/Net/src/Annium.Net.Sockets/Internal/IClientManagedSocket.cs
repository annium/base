using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Net.Sockets.Internal;

internal interface IClientManagedSocket : ISendingReceivingSocket
{
    Task<SocketCloseResult> IsClosed { get; }
    Task<Exception?> ConnectAsync(IPEndPoint endpoint, CancellationToken ct);
    Task DisconnectAsync();
}