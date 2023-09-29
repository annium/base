using System;
using System.Net;
using Annium.Logging;

namespace Annium.Net.Sockets;

public interface IClientSocket : ISendingReceivingSocket, ILogSubject
{
    event Action OnConnected;
    event Action<SocketCloseStatus> OnDisconnected;
    event Action<Exception> OnError;
    void Connect(IPEndPoint endpoint);
    void Disconnect();
}