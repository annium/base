using System;
using Annium.Logging;

namespace Annium.Net.WebSockets;

public interface IClientWebSocket : ISendingReceivingWebSocket, ILogSubject
{
    event Action OnConnected;
    event Action<WebSocketCloseStatus> OnDisconnected;
    event Action<Exception> OnError;
    void Connect(Uri uri);
    void Disconnect();
}