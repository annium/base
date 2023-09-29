using System;

namespace Annium.Net.WebSockets;

public interface IConnectionMonitor
{
    event Action OnConnectionLost;
    void Init(ISendingReceivingWebSocket socket);
    void Start();
    void Stop();
}