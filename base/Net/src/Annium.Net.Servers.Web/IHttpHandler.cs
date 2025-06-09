using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Net.Servers.Web;

/// <summary>
/// Defines a contract for handling HTTP requests in a web server.
/// </summary>
public interface IHttpHandler
{
    /// <summary>
    /// Handles an incoming HTTP request asynchronously.
    /// </summary>
    /// <param name="ctx">The HTTP listener context containing the request and response.</param>
    /// <param name="ct">The cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleAsync(HttpListenerContext ctx, CancellationToken ct);
}
