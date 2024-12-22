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
    public Task<WebSocketCloseResult> IsClosed { get; private set; } =
        Task.FromResult(new WebSocketCloseResult(WebSocketCloseStatus.ClosedLocal, null));
    private readonly int _keepAliveInterval;
    private readonly Lock _locker = new();
    private Connection? _cn;
    private CancellationTokenSource _listenCts = new();

    public ClientManagedWebSocket(int keepAliveInterval, ILogger logger)
    {
        _keepAliveInterval = keepAliveInterval;
        Logger = logger;
    }

    public void Dispose()
    {
        this.Trace("start");

        lock (_locker)
        {
            var cn = Interlocked.Exchange(ref _cn, null);
            if (cn is null)
            {
                this.Trace("skip - not connected");
                return;
            }

            this.Trace("unbind events");
            cn.Managed.OnBinaryReceived -= HandleOnBinaryReceived;
            cn.Managed.OnTextReceived -= HandleOnTextReceived;

            this.Trace("cancel listen cts");
            _listenCts.Cancel();
            _listenCts.Dispose();

            this.Trace("dispose connection");
            cn.Dispose();
        }
    }

    public async Task<Exception?> ConnectAsync(Uri uri, CancellationToken ct = default)
    {
        this.Trace("start");

        // only connection is checked, because after disconnect listen task can still be awaited
        if (_cn is not null)
            throw new InvalidOperationException("Socket is already connected");

        var nativeSocket = new NativeWebSocket
        {
            Options = { KeepAliveInterval = TimeSpan.FromMilliseconds(_keepAliveInterval) },
        };
        var managedSocket = new ManagedWebSocket(nativeSocket, Logger);
        this.Trace<string, string>(
            "paired with {nativeSocket} / {managedSocket}",
            nativeSocket.GetFullId(),
            managedSocket.GetFullId()
        );

        this.Trace("bind events");
        managedSocket.OnTextReceived += HandleOnTextReceived;
        managedSocket.OnBinaryReceived += HandleOnBinaryReceived;

        try
        {
            this.Trace("connect native socket to {uri}", uri);
            await nativeSocket.ConnectAsync(uri, ct);

            var cn = new Connection(nativeSocket, managedSocket, Logger);

            lock (_locker)
            {
                if (ct.IsCancellationRequested)
                {
                    this.Trace("connection canceled, dispose");
#pragma warning disable VSTHRD103
                    cn.Dispose();
#pragma warning restore VSTHRD103

                    return null;
                }

                this.Trace("save connection");
                _cn = cn;

                this.Trace("create listen cts");
                _listenCts = new CancellationTokenSource();

                this.Trace("create listen task");
                IsClosed = managedSocket
                    .ListenAsync(_listenCts.Token)
                    .ContinueWith(HandleClosed, CancellationToken.None);
            }

            this.Trace("done (connected)");

            return null;
        }
        catch (Exception e)
        {
            this.Trace("failed: {e}", e);

            Cleanup(nativeSocket, managedSocket);

            this.Trace("done (not connected)");

            return e;
        }
    }

    public async Task DisconnectAsync()
    {
        this.Trace("start");

        Connection? cn;
        lock (_locker)
        {
            cn = Interlocked.Exchange(ref _cn, null);
            if (cn is null)
            {
                this.Trace("skip - not connected");
                return;
            }

            this.Trace("unbind events");
            cn.Managed.OnTextReceived -= HandleOnTextReceived;
            cn.Managed.OnBinaryReceived -= HandleOnBinaryReceived;

            this.Trace("cancel listen cts");
#pragma warning disable VSTHRD103
            _listenCts.Cancel();
            _listenCts.Dispose();
#pragma warning restore VSTHRD103
        }

        try
        {
            this.Trace("close output");
            if (cn.Native.State is WebSocketState.Open or WebSocketState.CloseReceived)
                await cn.Native.CloseOutputAsync(
                    System.Net.WebSockets.WebSocketCloseStatus.NormalClosure,
                    string.Empty,
                    CancellationToken.None
                );
        }
        catch (Exception e)
        {
            this.Trace("failed: {e}", e);
        }

        this.Trace("await listen task");
#pragma warning disable VSTHRD003
        await IsClosed;
#pragma warning restore VSTHRD003

        this.Trace("done");
    }

    public ValueTask<WebSocketSendStatus> SendTextAsync(ReadOnlyMemory<byte> text, CancellationToken ct = default)
    {
        this.Trace("send text");

        return _cn?.Managed.SendTextAsync(text, ct) ?? ValueTask.FromResult(WebSocketSendStatus.Closed);
    }

    public ValueTask<WebSocketSendStatus> SendBinaryAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        this.Trace("send binary");

        return _cn?.Managed.SendBinaryAsync(data, ct) ?? ValueTask.FromResult(WebSocketSendStatus.Closed);
    }

    private WebSocketCloseResult HandleClosed(Task<WebSocketCloseResult> task)
    {
        this.Trace("start");

        if (task.Exception is not null)
            this.Error(task.Exception);

#pragma warning disable VSTHRD002
        lock (_locker)
        {
            var cn = Interlocked.Exchange(ref _cn, null);
            if (cn is null)
            {
                this.Trace("already not connected");
                return task.Result;
            }

            this.Trace("start, unsubscribe from managed socket");
            cn.Managed.OnTextReceived -= HandleOnTextReceived;
            cn.Managed.OnBinaryReceived -= HandleOnBinaryReceived;
        }

        this.Trace("done");

        return task.Result;
#pragma warning restore VSTHRD002
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

    private void Cleanup(NativeWebSocket nativeSocket, ManagedWebSocket managedSocket)
    {
        this.Trace("start, dispose native socket");
        nativeSocket.Dispose();

        this.Trace("unbind events");
        managedSocket.OnTextReceived -= HandleOnTextReceived;
        managedSocket.OnBinaryReceived -= HandleOnBinaryReceived;

        this.Trace("done");
    }

    private sealed record Connection(NativeWebSocket Native, ManagedWebSocket Managed, ILogger Logger)
        : IDisposable,
            ILogSubject
    {
        public void Dispose()
        {
            try
            {
                this.Trace("dispose native socket");
                Native.Dispose();
            }
            catch (Exception e)
            {
                this.Trace("failed: {e}", e);
            }
        }
    }
}
