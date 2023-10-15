using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using NativeWebSocket = System.Net.WebSockets.ClientWebSocket;

namespace Annium.Net.WebSockets.Internal;

internal class ClientManagedWebSocket : IClientManagedWebSocket, ILogSubject
{
    public ILogger Logger { get; }
    public event Action<ReadOnlyMemory<byte>> OnTextReceived = delegate { };
    public event Action<ReadOnlyMemory<byte>> OnBinaryReceived = delegate { };
    public Task<WebSocketCloseResult> IsClosed => _listenTask;
    private NativeWebSocket? _nativeSocket;
    private ManagedWebSocket? _managedSocket;
    private CancellationTokenSource _listenCts = new();
    private Task<WebSocketCloseResult> _listenTask = Task.FromResult(new WebSocketCloseResult(WebSocketCloseStatus.ClosedLocal, null));

    public ClientManagedWebSocket(ILogger logger)
    {
        Logger = logger;
    }

    public async Task<Exception?> ConnectAsync(Uri uri, CancellationToken ct = default)
    {
        this.Trace("start");

        // only sockets are checked, because after disconnect listen task can still be awaited
        if (_nativeSocket is not null || _managedSocket is not null)
            throw new InvalidOperationException("Socket is already connected");

        _nativeSocket = new NativeWebSocket();
        _managedSocket = new ManagedWebSocket(_nativeSocket, Logger);
        this.Trace<string, string>("paired with {nativeSocket} / {managedSocket}", _nativeSocket.GetFullId(), _managedSocket.GetFullId());

        this.Trace("bind events");
        _managedSocket.OnTextReceived += HandleOnTextReceived;
        _managedSocket.OnBinaryReceived += HandleOnBinaryReceived;

        try
        {
            this.Trace("connect native socket to {uri}", uri);
            await _nativeSocket.ConnectAsync(uri, ct);
        }
        catch (Exception e)
        {
            this.Trace("failed: {e}", e);

            this.Trace("dispose native socket");
            _nativeSocket.Dispose();
            _nativeSocket = null;

            this.Trace("unbind events");
            _managedSocket.OnTextReceived -= HandleOnTextReceived;
            _managedSocket.OnBinaryReceived -= HandleOnBinaryReceived;
            _managedSocket = null;

            this.Trace("done (not connected)");

            return e;
        }

        this.Trace("create listen cts");
        _listenCts = new CancellationTokenSource();

        this.Trace("create listen task");
        _listenTask = _managedSocket.ListenAsync(_listenCts.Token).ContinueWith(HandleClosed, CancellationToken.None);

        this.Trace("done (connected)");

        return null;
    }

    public async Task DisconnectAsync()
    {
        this.Trace("start");

        if (_nativeSocket is null || _managedSocket is null)
        {
            this.Trace("skip - not connected");
            return;
        }

        this.Trace("unbind events");
        _managedSocket.OnTextReceived -= HandleOnTextReceived;
        _managedSocket.OnBinaryReceived -= HandleOnBinaryReceived;

        try
        {
            this.Trace("close output");
            if (_nativeSocket.State is WebSocketState.Open or WebSocketState.CloseReceived)
                await _nativeSocket.CloseOutputAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
        }
        catch (Exception e)
        {
            this.Trace("failed: {e}", e);
        }

        this.Trace("cancel listen cts");
        _listenCts.Cancel();
        _listenCts.Dispose();

        this.Trace("await listen task");
        await _listenTask;

        this.Trace("reset socket references to null");
        _nativeSocket = null;
        _managedSocket = null;

        this.Trace("done");
    }

    public ValueTask<WebSocketSendStatus> SendTextAsync(ReadOnlyMemory<byte> text, CancellationToken ct = default)
    {
        this.Trace("send text");

        return _managedSocket?.SendTextAsync(text, ct) ?? ValueTask.FromResult(WebSocketSendStatus.Closed);
    }

    public ValueTask<WebSocketSendStatus> SendBinaryAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        this.Trace("send binary");

        return _managedSocket?.SendBinaryAsync(data, ct) ?? ValueTask.FromResult(WebSocketSendStatus.Closed);
    }

    private WebSocketCloseResult HandleClosed(Task<WebSocketCloseResult> task)
    {
        this.Trace("start");

        if (task.Exception is not null)
            this.Error(task.Exception);

        if (_managedSocket is not null)
        {
            this.Trace("start, unsubscribe from managed socket");
            _managedSocket.OnTextReceived -= HandleOnTextReceived;
            _managedSocket.OnBinaryReceived -= HandleOnBinaryReceived;
        }

        this.Trace("reset socket references to null");
        _nativeSocket = null;
        _managedSocket = null;

        this.Trace("done");

        return task.Result;
    }

    private void HandleOnTextReceived(ReadOnlyMemory<byte> data)
    {
        this.Trace("trigger text received");
        OnTextReceived(data);
    }

    private void HandleOnBinaryReceived(ReadOnlyMemory<byte> data)
    {
        this.Trace("trigger binary received");
        OnBinaryReceived(data);
    }
}