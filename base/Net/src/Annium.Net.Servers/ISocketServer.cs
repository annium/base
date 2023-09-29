using System.Threading;
using System.Threading.Tasks;

namespace Annium.Net.Servers;

public interface ISocketServer
{
    Task RunAsync(CancellationToken ct = default);
}