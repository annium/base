using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using NativeWebSocket = System.Net.WebSockets.WebSocket;

namespace Annium.Net.WebSockets.Internal;

internal class ServerManagedWebSocket : IServerManagedWebSocket, ILogSubject
{
    public ILogger Logger { get; }
    public event Action<ReadOnlyMemory<byte>> OnTextReceived = delegate { };
    public event Action<ReadOnlyMemory<byte>> OnBinaryReceived = delegate { };
    public Task<WebSocketCloseResult> IsClosed { get; }
    private readonly NativeWebSocket _nativeSocket;
    private readonly ManagedWebSocket _managedSocket;

    public ServerManagedWebSocket(NativeWebSocket nativeSocket, ILogger logger, CancellationToken ct = default)
    {
        Logger = logger;
        _nativeSocket = nativeSocket;
        _managedSocket = new ManagedWebSocket(nativeSocket, logger);
        this.Trace<string, string>(
            "paired with {nativeSocket} / {managedSocket}",
            _nativeSocket.GetFullId(),
            _managedSocket.GetFullId()
        );

        _managedSocket.OnTextReceived += HandleOnTextReceived;
        _managedSocket.OnBinaryReceived += HandleOnBinaryReceived;

        this.Trace("start listen");
        IsClosed = _managedSocket.ListenAsync(ct).ContinueWith(HandleClosed);
    }

    public async Task DisconnectAsync()
    {
        this.Trace("start");

        this.Trace("unbind events");
        _managedSocket.OnTextReceived -= HandleOnTextReceived;
        _managedSocket.OnBinaryReceived -= HandleOnBinaryReceived;

        try
        {
            this.Trace("close output");
            if (_nativeSocket.State is WebSocketState.Open or WebSocketState.CloseReceived)
                await _nativeSocket.CloseOutputAsync(
                    System.Net.WebSockets.WebSocketCloseStatus.NormalClosure,
                    string.Empty,
                    CancellationToken.None
                );
        }
        catch (Exception e)
        {
            this.Trace("failed: {e}", e);
        }

        this.Trace("done");
    }

    public ValueTask<WebSocketSendStatus> SendTextAsync(ReadOnlyMemory<byte> text, CancellationToken ct = default)
    {
        this.Trace("send text");

        return _managedSocket.SendTextAsync(text, ct);
    }

    public ValueTask<WebSocketSendStatus> SendBinaryAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        this.Trace("send binary");

        return _managedSocket.SendBinaryAsync(data, ct);
    }

    private WebSocketCloseResult HandleClosed(Task<WebSocketCloseResult> task)
    {
        this.Trace("start, unsubscribe from managed socket");

        if (task.Exception is not null)
            this.Error(task.Exception);

        _managedSocket.OnTextReceived -= HandleOnTextReceived;
        _managedSocket.OnBinaryReceived -= HandleOnBinaryReceived;

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
