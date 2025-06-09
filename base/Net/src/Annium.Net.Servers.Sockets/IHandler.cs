using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Net.Servers.Sockets;

/// <summary>
/// Defines a contract for handling socket connections in a socket server
/// </summary>
public interface IHandler
{
    /// <summary>
    /// Handles an incoming socket connection asynchronously
    /// </summary>
    /// <param name="socket">The socket connection to handle</param>
    /// <param name="ct">Cancellation token to cancel the operation</param>
    /// <returns>A task that represents the asynchronous handling operation</returns>
    Task HandleAsync(Socket socket, CancellationToken ct);
}
