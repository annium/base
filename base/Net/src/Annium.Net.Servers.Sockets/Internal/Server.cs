using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Execution;
using Annium.Logging;

namespace Annium.Net.Servers.Sockets.Internal;

internal class Server : IServer, ILogSubject
{
    public ILogger Logger { get; }
    private readonly TcpListener _listener;
    private readonly IBackgroundExecutor _executor;
    private readonly IHandler _handler;
    private int _isListening;

    public Server(
        int port,
        IHandler handler,
        ILogger logger
    )
    {
        Logger = logger;
        _listener = new TcpListener(IPAddress.Any, port);
        _executor = Executor.Background.Parallel<Server>(Logger);
        _handler = handler;
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
            await _handler.HandleAsync(socket, ct).ConfigureAwait(false);
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