using System;
using Annium.Logging;

namespace Annium.Net.WebSockets;

/// <summary>
/// Represents a server-side WebSocket connection for handling client connections
/// </summary>
public interface IServerWebSocket : ISendingReceivingWebSocket, ILogSubject
{
    /// <summary>
    /// Event triggered when the WebSocket connection is closed
    /// </summary>
    event Action<WebSocketCloseStatus> OnDisconnected;

    /// <summary>
    /// Event triggered when an error occurs on the WebSocket connection
    /// </summary>
    event Action<Exception> OnError;

    /// <summary>
    /// Closes the WebSocket connection
    /// </summary>
    void Disconnect();
}
