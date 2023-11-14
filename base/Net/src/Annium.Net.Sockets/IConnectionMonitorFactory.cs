namespace Annium.Net.Sockets;

public interface IConnectionMonitorFactory
{
    public IConnectionMonitor Create(ISendingReceivingSocket socket);
}