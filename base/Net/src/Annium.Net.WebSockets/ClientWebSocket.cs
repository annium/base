using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.WebSockets.Internal;

namespace Annium.Net.WebSockets;

public class ClientWebSocket : IClientWebSocket
{
    public ILogger Logger { get; }
    public event Action<ReadOnlyMemory<byte>> TextReceived = delegate { };
    public event Action<ReadOnlyMemory<byte>> BinaryReceived = delegate { };
    public event Action OnConnected = delegate { };
    public event Action<WebSocketCloseStatus> OnDisconnected = delegate { };
    public event Action<Exception> OnError = delegate { };
    private Uri Uri => _uri ?? throw new InvalidOperationException("Uri is not set");
    private readonly object _locker = new();
    private readonly IClientManagedWebSocket _socket;
    private readonly IConnectionMonitor _connectionMonitor;
    private Uri? _uri;
    private Status _status = Status.Disconnected;
    private readonly int _reconnectDelay;

    public ClientWebSocket(ClientWebSocketOptions options, ILogger logger)
    {
        Logger = logger;
        this.Trace("start monitor");
        _socket = new ClientManagedWebSocket(logger);
        _socket.TextReceived += OnTextReceived;
        _socket.BinaryReceived += OnBinaryReceived;
        this.Trace<string>("paired with {socket}", _socket.GetFullId());

        this.Trace("init monitor");
        _connectionMonitor = options.ConnectionMonitor;
        _connectionMonitor.Init(this);
        _reconnectDelay = options.ReconnectDelay;

        this.Trace("subscribe to OnConnectionLost");
        _connectionMonitor.OnConnectionLost += HandleConnectionLost;
    }

    public ClientWebSocket(
        ILogger logger
    ) : this(
        ClientWebSocketOptions.Default,
        logger
    )
    {
    }

    public void Connect(Uri uri)
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

        ConnectPrivate(uri);

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
        OnDisconnected(WebSocketCloseStatus.ClosedLocal);

        this.Trace("done");
    }

    public ValueTask<WebSocketSendStatus> SendTextAsync(ReadOnlyMemory<byte> text, CancellationToken ct = default)
    {
        this.Trace("send text");
        return _socket.SendTextAsync(text, ct);
    }

    public ValueTask<WebSocketSendStatus> SendBinaryAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        this.Trace("send binary");
        return _socket.SendBinaryAsync(data, ct);
    }

    private void ReconnectPrivate(Uri uri, WebSocketCloseResult result)
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
            ConnectPrivate(uri);

            this.Trace("done");
        });
    }

    private void ConnectPrivate(Uri uri)
    {
        this.Trace("start");

        _uri = uri;
        this.Trace("connect to {uri}", uri);
        _socket.ConnectAsync(uri, CancellationToken.None).ContinueWith(HandleConnected, uri);

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
            var uri = (Uri)state!;
            this.Trace("failure: {exception}, init reconnect", task.Exception);
            ReconnectPrivate(uri, new WebSocketCloseResult(WebSocketCloseStatus.Error, task.Result));
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

        ReconnectPrivate(Uri, new WebSocketCloseResult(WebSocketCloseStatus.ClosedRemote, null));

        this.Trace("done");
    }

    private void HandleClosed(Task<WebSocketCloseResult> task)
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

        ReconnectPrivate(Uri, task.Result);

        this.Trace("done");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetStatus(Status status)
    {
        this.Trace("update status from {currentStatus} to {newStatus}", _status, status);
        _status = status;
    }

    private void OnTextReceived(ReadOnlyMemory<byte> data)
    {
        this.Trace("trigger text received");
        TextReceived(data);
    }

    private void OnBinaryReceived(ReadOnlyMemory<byte> data)
    {
        this.Trace("trigger binary received");
        BinaryReceived(data);
    }

    private enum Status
    {
        Disconnected,
        Connecting,
        Connected,
    }
}