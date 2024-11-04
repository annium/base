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
    private readonly IManagedSocket _socket;

    public ServerManagedSocket(Stream stream, ManagedSocketOptions options, ILogger logger, CancellationToken ct)
    {
        Logger = logger;
        _stream = stream;
        _socket = Helper.GetManagedSocket(stream, options, logger);
        this.Trace<string, string>(
            "paired with {nativeSocket} / {managedSocket}",
            _stream.GetFullId(),
            _socket.GetFullId()
        );

        _socket.OnReceived += HandleOnReceived;

        this.Trace("start listen");
        IsClosed = _socket.ListenAsync(ct).ContinueWith(HandleClosed);
    }

    public void Dispose()
    {
        this.Trace("start, dispose socket");

        _socket.Dispose();

        this.Trace("done");
    }

    public async Task DisconnectAsync()
    {
        this.Trace("start");

        this.Trace("unbind events");
        _socket.OnReceived -= HandleOnReceived;

        try
        {
            this.Trace("dispose socket");
            await _socket.DisposeAsync();

            this.Trace("close stream");
            _stream.Close();
        }
        catch (Exception e)
        {
            this.Trace("failed: {e}", e);
        }

        this.Trace("done");
    }

    public ValueTask<SocketSendStatus> SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        this.Trace("send binary");

        return _socket.SendAsync(data, ct);
    }

    private SocketCloseResult HandleClosed(Task<SocketCloseResult> task)
    {
        this.Trace("start, unsubscribe from managed socket");

        if (task.Exception is not null)
            this.Error(task.Exception);

        _socket.OnReceived -= HandleOnReceived;

        this.Trace("done");

#pragma warning disable VSTHRD002
        return task.Result;
#pragma warning restore VSTHRD002
    }

    private void HandleOnReceived(ReadOnlyMemory<byte> data)
    {
        this.Trace("trigger binary received");
        OnReceived(data);
    }
}
