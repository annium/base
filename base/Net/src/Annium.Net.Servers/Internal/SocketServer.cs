using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Extensions.Execution;
using Annium.Logging;

namespace Annium.Net.Servers.Internal;

internal class SocketServer : ISocketServer, ILogSubject
{
    public ILogger Logger { get; }
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
        Logger = sp.Resolve<ILogger>();
        _sp = sp;
        _listener = new TcpListener(IPAddress.Any, port);
        _handle = handle;
        _executor = Executor.Background.Parallel<SocketServer>(Logger);
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        this.Trace("start");

        if (Interlocked.CompareExchange(ref _isListening, 1, 0) == 1)
            throw new InvalidOperationException("Server is already started");

        this.Trace("start executor");
        _executor.Start(ct);

        this.Trace("start listener");
        _listener.Start();

        while (!ct.IsCancellationRequested)
        {
            Socket socket;
            try
            {
                // await for connection
                socket = await _listener.AcceptSocketAsync(ct);
                this.Trace("socket accepted");
            }
            catch (OperationCanceledException)
            {
                this.Trace("break, operation canceled");
                break;
            }

            // try schedule socket handling
            if (_executor.TrySchedule(HandleSocket(socket, ct)))
            {
                this.Trace("socket handle scheduled");
                continue;
            }

            this.Trace("closed and dispose socket (server is already stopping)");
            socket.Close();
            socket.Dispose();
        }

        // when cancelled - await connections processing and stop listener
        this.Trace("dispose executor");
        await _executor.DisposeAsync().ConfigureAwait(false);

        this.Trace("stop listener");
        _listener.Stop();
    }

    private Func<ValueTask> HandleSocket(Socket socket, CancellationToken ct) => async () =>
    {
        try
        {
            this.Trace("handle socket");
            await _handle(_sp, socket, ct).ConfigureAwait(false);
        }
        finally
        {
            if (socket.Connected)
            {
                this.Trace("close socket");
                socket.Close();
            }
            else
                this.Trace("socket is not connected already");

            this.Trace("dispose socket");
            socket.Dispose();
        }
    };
}