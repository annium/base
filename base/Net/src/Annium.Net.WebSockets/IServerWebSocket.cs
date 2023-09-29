using System;
using Annium.Logging;

namespace Annium.Net.WebSockets;

public interface IServerWebSocket : ISendingReceivingWebSocket, ILogSubject
{
    event Action<WebSocketCloseStatus> OnDisconnected;
    event Action<Exception> OnError;
    void Disconnect();
}