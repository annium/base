using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Net.Sockets.Internal;

internal class ClientManagedSocket : IClientManagedSocket, ILogSubject
{
    public ILogger Logger { get; }
    public event Action<ReadOnlyMemory<byte>> OnReceived = delegate { };

    public Task<SocketCloseResult> IsClosed
    {
        get
        {
            if (_listenTask is null)
                throw new InvalidOperationException("Socket is not connected");

            return _listenTask;
        }
    }

    private Stream? _stream;
    private ManagedSocket? _socket;
    private CancellationTokenSource? _listenCts;
    private Task<SocketCloseResult>? _listenTask;

    public ClientManagedSocket(ILogger logger)
    {
        Logger = logger;
    }

    public async Task<Exception?> ConnectAsync(
        IPEndPoint endpoint,
        SslClientAuthenticationOptions? authOptions,
        CancellationToken ct = default
    )
    {
        this.Trace("start");

        // only sockets are checked, because after disconnect listen task can still be awaited
        if (_stream is not null || _socket is not null)
            throw new InvalidOperationException("Socket is already connected");

        try
        {
            this.Trace("create native socket");
            var nativeSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);

            this.Trace("connect native socket to {endpoint}", endpoint);
            await nativeSocket.ConnectAsync(endpoint, ct);

            if (authOptions is not null)
            {
                this.Trace("wrap native socket with network stream");
                var networkStream = new NetworkStream(nativeSocket);

                this.Trace("wrap network stream with ssl stream");
                var sslStream = new SslStream(
                    networkStream,
                    false,
                    authOptions.RemoteCertificateValidationCallback,
                    null
                );
                _stream = sslStream;

                this.Trace("authenticate client");
                await sslStream.AuthenticateAsClientAsync(authOptions, ct);
            }
            else
            {
                this.Trace("wrap native socket with network stream");
                _stream = new NetworkStream(nativeSocket);
            }

            this.Trace("wrap to managed socket");
            _socket = new ManagedSocket(_stream, Logger);
            this.Trace<string, string>("paired with {nativeSocket} / {managedSocket}", _stream.GetFullId(), _socket.GetFullId());

            this.Trace("bind events");
            _socket.OnReceived += HandleOnReceived;
        }
        catch (Exception e)
        {
            this.Error("failed: {e}", e);

            this.Trace("dispose native socket");
            if (_stream is not null)
            {
                _stream.Close();
                _stream = null;
            }

            this.Trace("unbind events");
            if (_socket is not null)
            {
                _socket.OnReceived -= HandleOnReceived;
                _socket = null;
            }

            this.Trace("done (not connected)");

            return e;
        }

        this.Trace("create listen cts");
        _listenCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        this.Trace("create listen task");
        _listenTask = _socket.ListenAsync(_listenCts.Token).ContinueWith(HandleClosed, CancellationToken.None);

        this.Trace("done (connected)");

        return null;
    }

    public async Task DisconnectAsync()
    {
        this.Trace("start");

        if (_stream is null || _socket is null || _listenCts is null || _listenTask is null)
        {
            this.Trace("skip - not connected");
            return;
        }

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

        this.Trace("cancel listen cts");
        _listenCts.Cancel();

        this.Trace("await listen task");
        await _listenTask;

        this.Trace("reset socket references to null");
        _stream = null;
        _socket = null;

        this.Trace("done");
    }

    public ValueTask<SocketSendStatus> SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        this.Trace("send");

        return _socket?.SendAsync(data, ct) ?? ValueTask.FromResult(SocketSendStatus.Closed);
    }

    private SocketCloseResult HandleClosed(Task<SocketCloseResult> task)
    {
        this.Trace("start");

        if (task.Exception is not null)
            this.Error(task.Exception);

        if (_socket is not null)
        {
            this.Trace("start, unsubscribe from managed socket");
            _socket.OnReceived -= HandleOnReceived;
        }

        this.Trace("reset socket references to null");
        _stream?.Close();
        _stream = null;
        _socket = null;

        this.Trace("done");

        return task.Result;
    }

    private void HandleOnReceived(ReadOnlyMemory<byte> data)
    {
        this.Trace("trigger binary received");
        OnReceived(data);
    }
}