namespace Annium.Net.Sockets;

public interface IConnectionMonitorFactory
{
    ConnectionMonitorBase Create(ISendingReceivingSocket socket, ConnectionMonitorOptions options);
}
