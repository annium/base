using System;
using System.Net;
using System.Net.Security;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.Sockets.Internal;

namespace Annium.Net.Sockets;

public class ClientSocket : IClientSocket
{
    public ILogger Logger { get; }
    public event Action<ReadOnlyMemory<byte>> OnReceived = delegate { };
    public event Action OnConnected = delegate { };
    public event Action<SocketCloseStatus> OnDisconnected = delegate { };
    public event Action<Exception> OnError = delegate { };
    private ConnectionConfig Config =>
        _connectionConfig ?? throw new InvalidOperationException("Connection config is not set");
    private readonly Lock _locker = new();
    private readonly IClientManagedSocket _socket;
    private readonly ConnectionMonitorBase _connectionMonitor;
    private readonly int _connectTimeout;
    private readonly int _reconnectDelay;
    private ConnectionConfig? _connectionConfig;
    private CancellationTokenSource _connectionCts = new();
    private Status _status = Status.Disconnected;

    public ClientSocket(ClientSocketOptions options, ILogger logger)
    {
        Logger = logger;
        this.Trace("start monitor");
        var managedOptions = new ManagedSocketOptions
        {
            Mode = options.Mode,
            BufferSize = options.BufferSize,
            ExtremeMessageSize = options.ExtremeMessageSize,
        };
        _socket = new ClientManagedSocket(managedOptions, logger);
        _socket.OnReceived += HandleOnReceived;
        this.Trace<string>("paired with {socket}", _socket.GetFullId());

        this.Trace("init monitor");
        _connectionMonitor =
            options.ConnectionMonitor.Factory?.Create(_socket, options.ConnectionMonitor)
            ?? new NoneConnectionMonitor(Logger);

        _connectTimeout = options.ConnectTimeout;
        _reconnectDelay = options.ReconnectDelay;

        this.Trace("subscribe to OnConnectionLost");
        _connectionMonitor.OnConnectionLost += HandleConnectionLost;
    }

    public ClientSocket(ILogger logger)
        : this(ClientSocketOptions.Default, logger) { }

    public void Connect(IPEndPoint endpoint, SslClientAuthenticationOptions? authOptions = null)
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

        ConnectPrivate(new(endpoint, authOptions));

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
            _connectionCts.Cancel();
            _connectionCts.Dispose();
        }

        this.Trace("stop monitor");
        _connectionMonitor.Stop();

        this.Trace("disconnect managed socket");
        _socket
            .DisconnectAsync()
            .ContinueWith(_ =>
            {
                this.Trace("fire disconnected");
                OnDisconnected(SocketCloseStatus.ClosedLocal);
            })
            .GetAwaiter();

        this.Trace("done");
    }

    public ValueTask<SocketSendStatus> SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        this.Trace("send binary");
        return _socket.SendAsync(data, ct);
    }

    public void Dispose()
    {
        Disconnect();
    }

    private void ReconnectPrivate(ConnectionConfig config, SocketCloseResult result)
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
        Task.Delay(_reconnectDelay)
            .ContinueWith(_ =>
            {
                this.Trace("trigger connect");
                ConnectPrivate(config);

                this.Trace("done");
            })
            .GetAwaiter();
    }

    private void ConnectPrivate(ConnectionConfig config)
    {
        this.Trace("start");

        lock (_locker)
        {
            if (_status is Status.Disconnected)
            {
                this.Trace("skip - already {status}", _status);
                return;
            }
        }

        _connectionConfig = config;
        this.Trace<IPEndPoint, string>(
            "connect to {endpoint} ({ssl})",
            config.Endpoint,
            config.AuthOptions is not null ? "ssl" : "plaintext"
        );
        var cts = new CancellationTokenSource(_connectTimeout);
        _connectionCts = cts;
        _socket
            .ConnectAsync(config.Endpoint, config.AuthOptions, cts.Token)
            .ContinueWith(HandleConnected, config, CancellationToken.None)
            .GetAwaiter();

        this.Trace("done");
    }

    private void HandleConnected(Task<Exception?> task, object? state)
    {
        this.Trace("start");

        if (task.Exception is not null)
            this.Error(task.Exception);

#pragma warning disable VSTHRD002
        lock (_locker)
        {
            if (_status is Status.Connected or Status.Disconnected)
            {
                this.Trace("skip - already {status}", _status);
                return;
            }

            // set status in lock
            this.Trace<string>(
                "set status by connection result: {result}",
                task.Result is null ? "ok" : task.Result.ToString()
            );
            SetStatus(task.Result is null ? Status.Connected : Status.Connecting);
        }

        if (task.Result is not null)
        {
            var config = (ConnectionConfig)state!;
            this.Trace("failure: {exception}, init reconnect", task.Exception);
            ReconnectPrivate(config, new SocketCloseResult(SocketCloseStatus.Error, task.Result));
            return;
        }
#pragma warning restore VSTHRD002

        this.Trace("subscribe to IsClosed");
        _socket.IsClosed.ContinueWith(HandleClosed, CancellationToken.None).GetAwaiter();

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

        this.Trace("disconnect managed socket");
        _socket.DisconnectAsync().GetAwaiter();

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

#pragma warning disable VSTHRD002
        ReconnectPrivate(Config, task.Result);
#pragma warning restore VSTHRD002

        this.Trace("done");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetStatus(Status status)
    {
        this.Trace("update status from {currentStatus} to {newStatus}", _status, status);
        _status = status;
    }

    private void HandleOnReceived(ReadOnlyMemory<byte> data)
    {
        if (data.Span.SequenceEqual(ProtocolFrames.Ping.Span))
        {
            this.Trace("skip ping frame");
            return;
        }

        this.Trace("trigger binary received");
        OnReceived(data);
    }

    private record ConnectionConfig(IPEndPoint Endpoint, SslClientAuthenticationOptions? AuthOptions);

    private enum Status
    {
        Disconnected,
        Connecting,
        Connected,
    }
}
