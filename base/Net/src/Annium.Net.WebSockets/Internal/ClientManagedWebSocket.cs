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
    public event Action<ReadOnlyMemory<byte>> TextReceived = delegate { };
    public event Action<ReadOnlyMemory<byte>> BinaryReceived = delegate { };

    public Task<WebSocketCloseResult> IsClosed
    {
        get
        {
            if (_listenTask is null)
                throw new InvalidOperationException("Socket is not connected");

            return _listenTask;
        }
    }

    private NativeWebSocket? _nativeSocket;
    private ManagedWebSocket? _managedSocket;
    private CancellationTokenSource? _listenCts;
    private Task<WebSocketCloseResult>? _listenTask;

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
        _managedSocket.TextReceived += OnTextReceived;
        _managedSocket.BinaryReceived += OnBinaryReceived;

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
            _managedSocket.TextReceived -= OnTextReceived;
            _managedSocket.BinaryReceived -= OnBinaryReceived;
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
        _managedSocket.TextReceived -= OnTextReceived;
        _managedSocket.BinaryReceived -= OnBinaryReceived;

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
            _managedSocket.TextReceived -= OnTextReceived;
            _managedSocket.BinaryReceived -= OnBinaryReceived;
        }

        this.Trace("reset socket references to null");
        _nativeSocket = null;
        _managedSocket = null;

        this.Trace("done");

        return task.Result;
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
}