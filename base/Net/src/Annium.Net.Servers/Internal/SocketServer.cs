using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Extensions.Execution;
using Annium.Logging;

namespace Annium.Net.Servers.Internal;

internal class SocketServer : ISocketServer
{
    private readonly IServiceProvider _sp;
    private readonly TcpListener _listener;
    private readonly Func<IServiceProvider, Socket, CancellationToken, Task> _handle;
    private readonly IBackgroundExecutor _executor;
    private int _isListening;

    public SocketServer(
        IServiceProvider sp,
        int port,
        Func<IServiceProvider, Socket, CancellationToken, Task> handle
    )
    {
        _sp = sp;
        _listener = new TcpListener(IPAddress.Any, port);
        _handle = handle;
        _executor = Executor.Background.Parallel<SocketServer>(_sp.Resolve<ILogger>());
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        if (Interlocked.CompareExchange(ref _isListening, 1, 0) == 1)
            throw new InvalidOperationException("Server is already started");

        _executor.Start(ct);
        _listener.Start();

        while (!ct.IsCancellationRequested)
        {
            Socket socket;
            try
            {
                // await for connection
                socket = await _listener.AcceptSocketAsync(ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            // schedule connection handling
            _executor.Schedule(async () =>
            {
                try
                {
                    await _handle(_sp, socket, ct).ConfigureAwait(false);
                }
                finally
                {
                    if (socket.Connected)
                        socket.Close();
                    socket.Dispose();
                }
            });
        }

        // when cancelled - await connections processing and stop listener
        await _executor.DisposeAsync().ConfigureAwait(false);
        _listener.Stop();
    }
}