using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Net.Sockets.Internal;

internal class ServerManagedSocket : IServerManagedSocket, ILogSubject
{
    public ILogger Logger { get; }
    public event Action<ReadOnlyMemory<byte>> OnReceived = delegate { };
    public Task<SocketCloseResult> IsClosed { get; }
    private readonly Stream _stream;
    private readonly ManagedSocket _socket;

    public ServerManagedSocket(
        Stream stream,
        ILogger logger,
        CancellationToken ct
    )
    {
        Logger = logger;
        _stream = stream;
        _socket = new ManagedSocket(stream, logger);
        this.Trace<string, string>("paired with {nativeSocket} / {managedSocket}", _stream.GetFullId(), _socket.GetFullId());

        _socket.OnReceived += HandleOnReceived;

        this.Trace("start listen");
        IsClosed = _socket.ListenAsync(ct).ContinueWith(HandleClosed);
    }

    public Task DisconnectAsync()
    {
        this.Trace("start");

        this.Trace("unbind events");
        _socket.OnReceived -= HandleOnReceived;

        try
        {
            this.Trace("close stream");
            _stream.Close();
        }
        catch (Exception e)
        {
            this.Trace("failed: {e}", e);
        }

        this.Trace("done");

        return Task.CompletedTask;
    }

    public ValueTask<SocketSendStatus> SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        this.Trace("send binary");

        return _socket.SendAsync(data, ct);
    }

    public void Dispose()
    {
        this.Trace("start");

        _stream.Close();

        this.Trace("done");
    }

    private SocketCloseResult HandleClosed(Task<SocketCloseResult> task)
    {
        this.Trace("start, unsubscribe from managed socket");

        if (task.Exception is not null)
            this.Error(task.Exception);

        _socket.OnReceived -= HandleOnReceived;

        this.Trace("done");

        return task.Result;
    }

    private void HandleOnReceived(ReadOnlyMemory<byte> data)
    {
        this.Trace("trigger binary received");
        OnReceived(data);
    }
}