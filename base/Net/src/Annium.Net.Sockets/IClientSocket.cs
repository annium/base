using System;
using System.Net;
using System.Net.Security;
using Annium.Logging;

namespace Annium.Net.Sockets;

/// <summary>
/// Represents a client-side socket that can connect to remote endpoints and send/receive data
/// </summary>
public interface IClientSocket : ISendingReceivingSocket, IDisposable, ILogSubject
{
    /// <summary>
    /// Indicates whether the socket is currently connected to a remote endpoint. Goes false
    /// synchronously when <see cref="Disconnect"/> begins; remains false through the
    /// background teardown and the eventual <see cref="OnDisconnected"/> firing — handlers
    /// observe a disconnected socket.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Event raised when the socket successfully connects to a remote endpoint
    /// </summary>
    event Action OnConnected;

    /// <summary>
    /// Event raised when the socket is disconnected from the remote endpoint
    /// </summary>
    event Action<SocketCloseStatus> OnDisconnected;

    /// <summary>
    /// Event raised when an error occurs during socket operations
    /// </summary>
    event Action<Exception> OnError;

    /// <summary>
    /// Connects to the specified remote endpoint
    /// </summary>
    /// <param name="endpoint">The remote endpoint to connect to</param>
    /// <param name="authOptions">Optional SSL client authentication options for secure connections</param>
    void Connect(IPEndPoint endpoint, SslClientAuthenticationOptions? authOptions = null);

    /// <summary>
    /// Disconnects from the remote endpoint
    /// </summary>
    void Disconnect();
}
