using System;
using System.Net;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Net.Sockets.Internal;

internal interface IClientManagedSocket : ISendingReceivingSocket, IDisposable
{
    Task<SocketCloseResult> IsClosed { get; }
    Task<Exception?> ConnectAsync(
        IPEndPoint endpoint,
        SslClientAuthenticationOptions? authOptions = null,
        CancellationToken ct = default
    );
    Task DisconnectAsync();
}
