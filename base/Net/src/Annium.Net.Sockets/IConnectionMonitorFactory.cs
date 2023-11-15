namespace Annium.Net.Sockets;

public interface IConnectionMonitorFactory
{
    public ConnectionMonitorBase Create(ISendingReceivingSocket socket);
}
