using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Net.Sockets.Internal;

internal class ClientManagedSocket : IClientManagedSocket, ILogSubject
{
    public ILogger Logger { get; }
    public event Action<ReadOnlyMemory<byte>> Received = delegate { };

    public Task<SocketCloseResult> IsClosed
    {
        get
        {
            if (_listenTask is null)
                throw new InvalidOperationException("Socket is not connected");

            return _listenTask;
        }
    }

    private Socket? _nativeSocket;
    private ManagedSocket? _managedSocket;
    private CancellationTokenSource? _listenCts;
    private Task<SocketCloseResult>? _listenTask;

    public ClientManagedSocket(ILogger logger)
    {
        Logger = logger;
    }

    public async Task<Exception?> ConnectAsync(IPEndPoint endpoint, CancellationToken ct = default)
    {
        this.Trace("start");

        // only sockets are checked, because after disconnect listen task can still be awaited
        if (_nativeSocket is not null || _managedSocket is not null)
            throw new InvalidOperationException("Socket is already connected");

        _nativeSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        _managedSocket = new ManagedSocket(_nativeSocket, Logger);
        this.Trace<string, string>("paired with {nativeSocket} / {managedSocket}", _nativeSocket.GetFullId(), _managedSocket.GetFullId());

        this.Trace("bind events");
        _managedSocket.Received += OnReceived;

        try
        {
            this.Trace("connect native socket to {endpoint}", endpoint);
            await _nativeSocket.ConnectAsync(endpoint, ct);
        }
        catch (Exception e)
        {
            this.Trace("failed: {e}", e);

            this.Trace("dispose native socket");
            _nativeSocket.Dispose();
            _nativeSocket = null;

            this.Trace("unbind events");
            _managedSocket.Received -= OnReceived;
            _managedSocket = null;

            this.Trace("done (not connected)");

            return e;
        }

        this.Trace("create listen cts");
        _listenCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        this.Trace("create listen task");
        _listenTask = _managedSocket.ListenAsync(_listenCts.Token).ContinueWith(HandleClosed, CancellationToken.None);

        this.Trace("done (connected)");

        return null;
    }

    public async Task DisconnectAsync()
    {
        this.Trace("start");

        if (_nativeSocket is null || _managedSocket is null || _listenCts is null || _listenTask is null)
        {
            this.Trace("skip - not connected");
            return;
        }

        this.Trace("unbind events");
        _managedSocket.Received -= OnReceived;

        try
        {
            if (_nativeSocket.Connected)
            {
                this.Trace("close socket");
                _nativeSocket.Close();
            }
            else
                this.Trace("close skipped - socket already closed");
        }
        catch (Exception e)
        {
            this.Trace("failed: {e}", e);
        }

        this.Trace("cancel listen cts");
        _listenCts.Cancel();

        this.Trace("await listen task");
        await _listenTask;

        this.Trace("reset socket references to null");
        _nativeSocket = null;
        _managedSocket = null;

        this.Trace("done");
    }

    public ValueTask<SocketSendStatus> SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        this.Trace("send");

        return _managedSocket?.SendAsync(data, ct) ?? ValueTask.FromResult(SocketSendStatus.Closed);
    }

    private SocketCloseResult HandleClosed(Task<SocketCloseResult> task)
    {
        this.Trace("start");

        if (task.Exception is not null)
            this.Error(task.Exception);

        if (_managedSocket is not null)
        {
            this.Trace("start, unsubscribe from managed socket");
            _managedSocket.Received -= OnReceived;
        }

        this.Trace("reset socket references to null");
        _nativeSocket = null;
        _managedSocket = null;

        this.Trace("done");

        return task.Result;
    }

    private void OnReceived(ReadOnlyMemory<byte> data)
    {
        this.Trace("trigger binary received");
        Received(data);
    }
}