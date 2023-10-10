using System.Threading;
using System.Threading.Tasks;

namespace Annium.Net.Sockets.Internal;

internal interface IManagedSocket : ISendingReceivingSocket
{
    Task<SocketCloseResult> ListenAsync(CancellationToken ct);
}