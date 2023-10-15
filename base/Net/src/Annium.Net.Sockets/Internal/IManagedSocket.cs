using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Net.Sockets.Internal;

internal interface IManagedSocket : ISendingReceivingSocket, IDisposable
{
    Task<SocketCloseResult> ListenAsync(CancellationToken ct);
}