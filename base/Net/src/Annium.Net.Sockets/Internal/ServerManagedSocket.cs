using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using NativeSocket = System.Net.Sockets.Socket;

namespace Annium.Net.Sockets.Internal;

internal class ServerManagedSocket : IServerManagedSocket, ILogSubject
{
    public ILogger Logger { get; }
    public event Action<ReadOnlyMemory<byte>> Received = delegate { };
    public Task<SocketCloseResult> IsClosed { get; }
    private readonly NativeSocket _nativeSocket;
    private readonly ManagedSocket _managedSocket;

    public ServerManagedSocket(
        NativeSocket nativeSocket,
        ILogger logger,
        CancellationToken ct = default
    )
    {
        Logger = logger;
        _nativeSocket = nativeSocket;
        _managedSocket = new ManagedSocket(nativeSocket, logger);
        this.Trace<string, string>("paired with {nativeSocket} / {managedSocket}", _nativeSocket.GetFullId(), _managedSocket.GetFullId());

        _managedSocket.Received += OnReceived;

        this.Trace("start listen");
        IsClosed = _managedSocket.ListenAsync(ct).ContinueWith(HandleClosed);
    }

    public Task DisconnectAsync()
    {
        this.Trace("start");

        this.Trace("unbind events");
        _managedSocket.Received -= OnReceived;

        try
        {
            this.Trace("close output");
            if (_nativeSocket.Connected)
                _nativeSocket.Close();
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

        return _managedSocket.SendAsync(data, ct);
    }

    private SocketCloseResult HandleClosed(Task<SocketCloseResult> task)
    {
        this.Trace("start, unsubscribe from managed socket");

        if (task.Exception is not null)
            this.Error(task.Exception);

        _managedSocket.Received -= OnReceived;

        this.Trace("done");

        return task.Result;
    }

    private void OnReceived(ReadOnlyMemory<byte> data)
    {
        this.Trace("trigger binary received");
        Received(data);
    }
}