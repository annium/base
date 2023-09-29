using System.Threading;
using System.Threading.Tasks;

namespace Annium.Net.Servers;

public interface IWebServer
{
    Task RunAsync(CancellationToken ct = default);
}