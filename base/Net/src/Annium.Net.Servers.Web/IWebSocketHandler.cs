using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Net.Servers.Web;

public interface IWebSocketHandler
{
    Task HandleAsync(HttpListenerWebSocketContext ctx, CancellationToken ct);
}
