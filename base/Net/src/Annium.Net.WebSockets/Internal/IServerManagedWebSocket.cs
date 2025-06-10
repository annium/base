using System;
using System.Threading.Tasks;

namespace Annium.Net.WebSockets.Internal;

/// <summary>
/// Internal interface for server-side managed WebSocket operations
/// </summary>
internal interface IServerManagedWebSocket : ISendingReceivingWebSocket, IDisposable
{
    /// <summary>
    /// Gets a task that completes when the WebSocket is closed, returning the close result
    /// </summary>
    Task<WebSocketCloseResult> IsClosed { get; }

    /// <summary>
    /// Disconnects the WebSocket
    /// </summary>
    /// <returns>A task that completes when the disconnection is finished</returns>
    Task DisconnectAsync();
}
