using System;
using System.Threading.Tasks;

namespace Annium.Net.WebSockets.Internal;

internal interface IServerManagedWebSocket : ISendingReceivingWebSocket, IDisposable
{
    Task<WebSocketCloseResult> IsClosed { get; }
    Task DisconnectAsync();
}
