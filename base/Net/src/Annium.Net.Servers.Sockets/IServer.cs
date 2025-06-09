using System.Threading;
using System.Threading.Tasks;

namespace Annium.Net.Servers.Sockets;

/// <summary>
/// Defines a contract for a socket server that can be started and run asynchronously
/// </summary>
public interface IServer
{
    /// <summary>
    /// Starts and runs the server asynchronously, listening for incoming connections
    /// </summary>
    /// <param name="ct">Cancellation token to stop the server</param>
    /// <returns>A task that represents the asynchronous server operation</returns>
    Task RunAsync(CancellationToken ct = default);
}
