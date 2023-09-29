using System.Threading.Tasks;

namespace Annium.Net.Sockets.Internal;

internal interface IServerManagedSocket : ISendingReceivingSocket
{
    Task<SocketCloseResult> IsClosed { get; }
    Task DisconnectAsync();
}