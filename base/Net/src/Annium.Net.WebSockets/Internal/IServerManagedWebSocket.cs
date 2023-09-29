using System.Threading.Tasks;

namespace Annium.Net.WebSockets.Internal;

internal interface IServerManagedWebSocket : ISendingReceivingWebSocket
{
    Task<WebSocketCloseResult> IsClosed { get; }
    Task DisconnectAsync();
}