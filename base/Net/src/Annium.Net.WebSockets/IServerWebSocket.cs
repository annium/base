using System;
using Annium.Logging;

namespace Annium.Net.WebSockets;

/// <summary>
/// Represents a server-side WebSocket connection for handling client connections.
/// </summary>
public interface IServerWebSocket : ISendingReceivingWebSocket, IDisposable, ILogSubject
{
    /// <summary>
    /// Indicates whether the WebSocket is currently connected. Goes false synchronously when
    /// <see cref="Disconnect"/> begins; remains false through the background teardown and the
    /// eventual <see cref="OnDisconnected"/> firing — handlers observe a disconnected socket.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Event triggered when the WebSocket connection is closed.
    /// </summary>
    event Action<WebSocketCloseStatus> OnDisconnected;

    /// <summary>
    /// Event triggered when an error occurs on the WebSocket connection.
    /// </summary>
    event Action<Exception> OnError;

    /// <summary>
    /// Closes the WebSocket connection.
    /// </summary>
    void Disconnect();
}
