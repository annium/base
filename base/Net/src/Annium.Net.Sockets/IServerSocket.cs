using System;
using Annium.Logging;

namespace Annium.Net.Sockets;

/// <summary>
/// Represents a server-side socket that handles incoming client connections.
/// </summary>
public interface IServerSocket : ISendingReceivingSocket, IDisposable, ILogSubject
{
    /// <summary>
    /// Indicates whether the socket is currently connected to the client. Goes false
    /// synchronously when <see cref="Disconnect"/> begins (or when the connection is closed);
    /// remains false through the background teardown and the eventual <see cref="OnDisconnected"/>
    /// firing — handlers observe a disconnected socket.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Event raised when the socket is disconnected from the client.
    /// </summary>
    event Action<SocketCloseStatus> OnDisconnected;

    /// <summary>
    /// Event raised when an error occurs during socket operations.
    /// </summary>
    event Action<Exception> OnError;

    /// <summary>
    /// Disconnects from the client.
    /// </summary>
    void Disconnect();
}
