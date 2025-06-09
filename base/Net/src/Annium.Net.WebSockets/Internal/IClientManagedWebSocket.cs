using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Net.WebSockets.Internal;

/// <summary>
/// Internal interface for client-side managed WebSocket operations
/// </summary>
internal interface IClientManagedWebSocket : ISendingReceivingWebSocket, IDisposable
{
    /// <summary>
    /// Gets a task that completes when the WebSocket is closed, returning the close result
    /// </summary>
    Task<WebSocketCloseResult> IsClosed { get; }

    /// <summary>
    /// Connects to the specified WebSocket URI
    /// </summary>
    /// <param name="uri">The WebSocket URI to connect to</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A task that completes when the connection is established, returning any connection exception</returns>
    Task<Exception?> ConnectAsync(Uri uri, CancellationToken ct);

    /// <summary>
    /// Disconnects the WebSocket
    /// </summary>
    /// <returns>A task that completes when the disconnection is finished</returns>
    Task DisconnectAsync();
}
