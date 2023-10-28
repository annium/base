using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Net.Servers.Sockets;

public interface IHandler
{
    Task HandleAsync(Socket socket, CancellationToken ct);
}
