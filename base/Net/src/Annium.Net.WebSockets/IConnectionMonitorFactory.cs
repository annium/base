namespace Annium.Net.WebSockets;

public interface IConnectionMonitorFactory
{
    ConnectionMonitorBase Create(ISendingReceivingWebSocket socket, ConnectionMonitorOptions options);
}
