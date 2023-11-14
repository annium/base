using System;

namespace Annium.Net.Sockets;

public interface IConnectionMonitor
{
    event Action OnConnectionLost;
    void Start();
    void Stop();
}
