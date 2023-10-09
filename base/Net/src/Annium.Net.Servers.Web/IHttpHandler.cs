using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Net.Servers.Web;

public interface IHttpHandler
{
    Task HandleAsync(HttpListenerContext ctx, CancellationToken ct);
}