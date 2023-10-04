using System;
using System.Threading.Tasks;

namespace Annium.Net.Sockets.Internal;

internal interface IServerManagedSocket : ISendingReceivingSocket, IDisposable
{
    Task<SocketCloseResult> IsClosed { get; }
    Task DisconnectAsync();
}