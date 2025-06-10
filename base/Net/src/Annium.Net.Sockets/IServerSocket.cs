using System;
using Annium.Logging;

namespace Annium.Net.Sockets;

/// <summary>
/// Represents a server-side socket that handles incoming client connections
/// </summary>
public interface IServerSocket : ISendingReceivingSocket, IDisposable, ILogSubject
{
    /// <summary>
    /// Event raised when the socket is disconnected from the client
    /// </summary>
    event Action<SocketCloseStatus> OnDisconnected;

    /// <summary>
    /// Event raised when an error occurs during socket operations
    /// </summary>
    event Action<Exception> OnError;

    /// <summary>
    /// Disconnects from the client
    /// </summary>
    void Disconnect();
}
