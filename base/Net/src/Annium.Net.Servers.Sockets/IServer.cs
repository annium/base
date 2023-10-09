using System.Threading;
using System.Threading.Tasks;

namespace Annium.Net.Servers.Sockets;

public interface IServer
{
    Task RunAsync(CancellationToken ct = default);
}