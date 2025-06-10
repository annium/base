using System;
using Annium.Logging;

namespace Annium.Net.WebSockets;

/// <summary>
/// Represents a client-side WebSocket connection with connection management capabilities
/// </summary>
public interface IClientWebSocket : ISendingReceivingWebSocket, IDisposable, ILogSubject
{
    /// <summary>
    /// Event triggered when the WebSocket connection is established
    /// </summary>
    event Action OnConnected;

    /// <summary>
    /// Event triggered when the WebSocket connection is closed
    /// </summary>
    event Action<WebSocketCloseStatus> OnDisconnected;

    /// <summary>
    /// Event triggered when an error occurs on the WebSocket connection
    /// </summary>
    event Action<Exception> OnError;

    /// <summary>
    /// Initiates a connection to the specified WebSocket URI
    /// </summary>
    /// <param name="uri">The WebSocket URI to connect to</param>
    void Connect(Uri uri);

    /// <summary>
    /// Closes the WebSocket connection
    /// </summary>
    void Disconnect();
}
