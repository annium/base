using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Net.Sockets.Internal;

/// <summary>
/// Internal interface for general managed socket operations
/// </summary>
internal interface IManagedSocket : ISendingReceivingSocket, IDisposable
{
    /// <summary>
    /// Starts listening for incoming data asynchronously
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A task that completes with the socket close result when listening ends</returns>
    Task<SocketCloseResult> ListenAsync(CancellationToken ct);
}
