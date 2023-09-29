using System;

namespace Annium.Net.Sockets;

public interface IConnectionMonitor
{
    event Action OnConnectionLost;
    void Init(ISendingReceivingSocket socket);
    void Start();
    void Stop();
}