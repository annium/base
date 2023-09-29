using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.Sockets.Internal;

namespace Annium.Net.Sockets;

public class ClientSocket : IClientSocket
{
    public ILogger Logger { get; }
    public event Action<ReadOnlyMemory<byte>> Received = delegate { };
    public event Action OnConnected = delegate { };
    public event Action<SocketCloseStatus> OnDisconnected = delegate { };
    public event Action<Exception> OnError = delegate { };
    private IPEndPoint Endpoint => _endpoint ?? throw new InvalidOperationException("Endpoint is not set");
    private readonly object _locker = new();
    private readonly IClientManagedSocket _socket;
    private readonly IConnectionMonitor _connectionMonitor;
    private IPEndPoint? _endpoint;
    private Status _status = Status.Disconnected;
    private readonly int _reconnectDelay;

    public ClientSocket(ClientSocketOptions options, ILogger logger)
    {
        Logger = logger;
        this.Trace("start monitor");
        _socket = new ClientManagedSocket(logger);
        _socket.Received += OnReceived;
        this.Trace<string>("paired with {socket}", _socket.GetFullId());

        this.Trace("init monitor");
        _connectionMonitor = options.ConnectionMonitor;
        _connectionMonitor.Init(this);
        _reconnectDelay = options.ReconnectDelay;

        this.Trace("subscribe to OnConnectionLost");
        _connectionMonitor.OnConnectionLost += HandleConnectionLost;
    }

    public ClientSocket(
        ILogger logger
    ) : this(
        ClientSocketOptions.Default,
        logger
    )
    {
    }

    public void Connect(IPEndPoint endpoint)
    {
        this.Trace("start");

        lock (_locker)
        {
            if (_status is Status.Connecting or Status.Connected)
            {
                this.Trace("skip - already {status}", _status);
                return;
            }

            SetStatus(Status.Connecting);
        }

        ConnectPrivate(endpoint);

        this.Trace("done");
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
        this.Trace("send binary");
        return _socket.SendAsync(data, ct);
    }

    private void ReconnectPrivate(IPEndPoint endpoint, SocketCloseResult result)
    {
        this.Trace("start");

        this.Trace("stop monitor");
        _connectionMonitor.Stop();

        if (result.Exception is not null)
        {
            this.Trace("fire error: {exception}", result.Exception);
            OnError(result.Exception);
        }

        this.Trace("fire disconnected with {closeStatus}", result.Status);
        OnDisconnected(result.Status);

        this.Trace("schedule connection in {reconnectDelay}ms", _reconnectDelay);
        Task.Delay(_reconnectDelay).ContinueWith(_ =>
        {
            this.Trace("trigger connect");
            ConnectPrivate(endpoint);

            this.Trace("done");
        });
    }

    private void ConnectPrivate(IPEndPoint endpoint)
    {
        this.Trace("start");

        _endpoint = endpoint;
        this.Trace("connect to {endpoint}", endpoint);
        _socket.ConnectAsync(endpoint, CancellationToken.None).ContinueWith(HandleConnected, endpoint);

        this.Trace("done");
    }

    private void HandleConnected(Task<Exception?> task, object? state)
    {
        this.Trace("start");

        if (task.Exception is not null)
            this.Error(task.Exception);

        lock (_locker)
        {
            if (_status is Status.Connected or Status.Disconnected)
            {
                this.Trace("skip - already {status}", _status);
                return;
            }

            // set status in lock
            this.Trace<string>("set status by connection result: {result}", task.Result is null ? "ok" : task.Result.ToString());
            SetStatus(task.Result is null ? Status.Connected : Status.Connecting);
        }

        if (task.Result is not null)
        {
            var endpoint = (IPEndPoint)state!;
            this.Trace("failure: {exception}, init reconnect", task.Exception);
            ReconnectPrivate(endpoint, new SocketCloseResult(SocketCloseStatus.Error, task.Result));
            return;
        }

        this.Trace("subscribe to IsClosed");
        _socket.IsClosed.ContinueWith(HandleClosed, CancellationToken.None);

        this.Trace("start monitor");
        _connectionMonitor.Start();

        this.Trace("fire connected");
        OnConnected();

        this.Trace("done");
    }

    private void HandleConnectionLost()
    {
        this.Trace("start");

        lock (_locker)
        {
            if (_status is Status.Disconnected)
            {
                this.Trace("skip - already {status}", _status);
                return;
            }

            SetStatus(Status.Connecting);
        }

        ReconnectPrivate(Endpoint, new SocketCloseResult(SocketCloseStatus.ClosedRemote, null));

        this.Trace("done");
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

            SetStatus(Status.Connecting);
        }

        ReconnectPrivate(Endpoint, task.Result);

        this.Trace("done");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetStatus(Status status)
    {
        this.Trace("update status from {currentStatus} to {newStatus}", _status, status);
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
        Connecting,
        Connected,
    }
}