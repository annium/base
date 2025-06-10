using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Net.Servers.Web;

/// <summary>
/// Defines a contract for handling WebSocket connections in a web server.
/// </summary>
public interface IWebSocketHandler
{
    /// <summary>
    /// Handles an incoming WebSocket connection asynchronously.
    /// </summary>
    /// <param name="ctx">The HTTP listener WebSocket context containing the WebSocket connection.</param>
    /// <param name="ct">The cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleAsync(HttpListenerWebSocketContext ctx, CancellationToken ct);
}
