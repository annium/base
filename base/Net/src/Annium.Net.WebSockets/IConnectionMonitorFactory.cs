using Annium.Net.WebSockets.Internal;

namespace Annium.Net.WebSockets;

public interface IConnectionMonitorFactory
{
    public ConnectionMonitorBase Create(ISendingReceivingWebSocket socket, ConnectionMonitorOptions options);
}
