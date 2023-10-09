using System.Threading;
using System.Threading.Tasks;

namespace Annium.Net.Servers.Web;

public interface IServer
{
    Task RunAsync(CancellationToken ct = default);
}