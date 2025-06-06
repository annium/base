using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Net.Sockets.Internal;

namespace Annium.Net.Sockets;

public class ServerSocket : IServerSocket
{
    public ILogger Logger { get; }
    public event Action<ReadOnlyMemory<byte>> OnReceived = delegate { };
    public event Action<SocketCloseStatus> OnDisconnected = delegate { };
    public event Action<Exception> OnError = delegate { };
    private readonly Lock _locker = new();
    private readonly IServerManagedSocket _socket;
    private readonly ConnectionMonitorBase _connectionMonitor;
    private Status _status = Status.Connected;

    public ServerSocket(Stream stream, ServerSocketOptions options, ILogger logger, CancellationToken ct = default)
    {
        Logger = logger;
        this.Trace("start");
        var managedOptions = new ManagedSocketOptions
        {
            Mode = options.Mode,
            BufferSize = options.BufferSize,
            ExtremeMessageSize = options.ExtremeMessageSize,
        };
        _socket = new ServerManagedSocket(stream, managedOptions, logger, ct);
        _socket.OnReceived += HandleOnReceived;
        this.Trace<string>("paired with {socket}", _socket.GetFullId());

        this.Trace("subscribe to IsClosed");
        _socket.IsClosed.ContinueWith(HandleClosed, CancellationToken.None).GetAwaiter();

        this.Trace("init monitor");
        _connectionMonitor =
            options.ConnectionMonitor.Factory?.Create(_socket, options.ConnectionMonitor)
            ?? new NoneConnectionMonitor(Logger);

        this.Trace("start monitor");
        _connectionMonitor.Start();

        this.Trace("subscribe to OnConnectionLost");
        _connectionMonitor.OnConnectionLost += Disconnect;
    }

    public ServerSocket(Stream stream, ILogger logger, CancellationToken ct = default)
        : this(stream, ServerSocketOptions.Default, logger, ct) { }

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
        this.Trace("send");
        return _socket.SendAsync(data, ct);
    }

    public void Dispose()
    {
        Disconnect();
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

#pragma warning disable VSTHRD002
        var result = task.Result;
#pragma warning restore VSTHRD002
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

    private enum Status
    {
        Disconnected,
        Connected,
    }
}
