using System;
using System.Threading.Tasks;

namespace Annium.Net.Sockets.Internal;

/// <summary>
/// Internal interface for server-side managed socket operations
/// </summary>
internal interface IServerManagedSocket : ISendingReceivingSocket, IDisposable
{
    /// <summary>
    /// Gets a task that completes when the socket is closed
    /// </summary>
    Task<SocketCloseResult> IsClosed { get; }

    /// <summary>
    /// Disconnects from the client asynchronously
    /// </summary>
    /// <returns>A task that completes when disconnection is finished</returns>
    Task DisconnectAsync();
}
