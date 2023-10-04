using System;
using System.Net;
using System.Net.Security;
using Annium.Logging;

namespace Annium.Net.Sockets;

public interface IClientSocket : ISendingReceivingSocket, IDisposable, ILogSubject
{
    event Action OnConnected;
    event Action<SocketCloseStatus> OnDisconnected;
    event Action<Exception> OnError;
    void Connect(IPEndPoint endpoint, SslClientAuthenticationOptions? authOptions = null);
    void Disconnect();
}