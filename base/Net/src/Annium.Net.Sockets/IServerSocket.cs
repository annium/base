using System;
using Annium.Logging;

namespace Annium.Net.Sockets;

public interface IServerSocket : ISendingReceivingSocket, IDisposable, ILogSubject
{
    event Action<SocketCloseStatus> OnDisconnected;
    event Action<Exception> OnError;
    void Disconnect();
}
