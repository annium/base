using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.Sockets.Internal;
using NativeSocket = System.Net.Sockets.Socket;

namespace Annium.Net.Sockets;

public class ServerSocket : IServerSocket
{
    public ILogger Logger { get; }
    public event Action<ReadOnlyMemory<byte>> Received = delegate { };
    public event Action<SocketCloseStatus> OnDisconnected = delegate { };
    public event Action<Exception> OnError = delegate { };
    private readonly object _locker = new();
    private readonly IServerManagedSocket _socket;
    private readonly IConnectionMonitor _connectionMonitor;
    private Status _status = Status.Connected;

    public ServerSocket(
        NativeSocket nativeSocket,
        ServerSocketOptions options,
        ILogger logger,
        CancellationToken ct = default
    )
    {
        Logger = logger;
        this.Trace("start");
        _socket = new ServerManagedSocket(nativeSocket, logger, ct);
        _socket.Received += OnReceived;
        this.Trace<string>("paired with {socket}", _socket.GetFullId());

        this.Trace("subscribe to IsClosed");
        _socket.IsClosed.ContinueWith(HandleClosed, CancellationToken.None);

        this.Trace("init monitor");
        _connectionMonitor = options.ConnectionMonitor;
        _connectionMonitor.Init(this);

        this.Trace("start monitor");
        _connectionMonitor.Start();

        this.Trace("subscribe to OnConnectionLost");
        _connectionMonitor.OnConnectionLost += Disconnect;
    }

    public ServerSocket(
        NativeSocket nativeSocket,
        ILogger logger,
        CancellationToken ct = default
    ) : this(
        nativeSocket,
        ServerSocketOptions.Default,
        logger,
        ct
    )
    {
    }

    public void Disconnect()
    {
        this.Trace("start");

        lock (_locker)
        {
            if (_status is Status.Disconnected)
            {
                this.Trace("skip - already {status}", _status);
                return;
            }

            SetStatus(Status.Disconnected);
        }

        this.Trace("stop monitor");
        _connectionMonitor.Stop();

        this.Trace("disconnect managed socket");
        _socket.DisconnectAsync();

        this.Trace("fire disconnected");
        OnDisconnected(SocketCloseStatus.ClosedLocal);

        this.Trace("done");
    }

    public ValueTask<SocketSendStatus> SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        this.Trace("send");
        return _socket.SendAsync(data, ct);
    }

    private void HandleClosed(Task<SocketCloseResult> task)
    {
        this.Trace("start");

        if (task.Exception is not null)
            this.Error(task.Exception);

        lock (_locker)
        {
            if (_status is Status.Disconnected)
            {
                this.Trace("skip - already {status}", _status);
                return;
            }

            SetStatus(Status.Disconnected);
        }

        this.Trace("stop monitor");
        _connectionMonitor.Stop();

        var result = task.Result;
        if (result.Exception is not null)
        {
            this.Trace("fire error: {exception}", result.Exception);
            OnError(result.Exception);
        }

        this.Trace("fire disconnected");
        OnDisconnected(result.Status);

        this.Trace("done");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetStatus(Status status)
    {
        this.Trace("update status from {oldStatus} to {newStatus}", _status, status);
        _status = status;
    }

    private void OnReceived(ReadOnlyMemory<byte> data)
    {
        this.Trace("trigger binary received");
        Received(data);
    }

    private enum Status
    {
        Disconnected,
        Connected,
    }
}