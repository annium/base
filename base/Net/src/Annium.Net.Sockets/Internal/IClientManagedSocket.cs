using System;
using System.Net;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Net.Sockets.Internal;

/// <summary>
/// Internal interface for client-side managed socket operations
/// </summary>
internal interface IClientManagedSocket : ISendingReceivingSocket, IDisposable
{
    /// <summary>
    /// Gets a task that completes when the socket is closed
    /// </summary>
    Task<SocketCloseResult> IsClosed { get; }

    /// <summary>
    /// Connects to the specified remote endpoint asynchronously
    /// </summary>
    /// <param name="endpoint">The remote endpoint to connect to</param>
    /// <param name="authOptions">Optional SSL client authentication options</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A task that completes with null on success or an exception on failure</returns>
    Task<Exception?> ConnectAsync(
        IPEndPoint endpoint,
        SslClientAuthenticationOptions? authOptions = null,
        CancellationToken ct = default
    );

    /// <summary>
    /// Disconnects from the remote endpoint asynchronously
    /// </summary>
    /// <returns>A task that completes when disconnection is finished</returns>
    Task DisconnectAsync();
}
