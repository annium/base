using System.Threading;
using System.Threading.Tasks;

namespace Annium.Net.Servers.Web;

/// <summary>
/// Defines a contract for a web server that can be started and stopped.
/// </summary>
public interface IServer
{
    /// <summary>
    /// Starts the web server and runs it until cancellation is requested.
    /// </summary>
    /// <param name="ct">The cancellation token to stop the server.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RunAsync(CancellationToken ct = default);
}
