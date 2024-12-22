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

    public Task<SocketCloseResult> IsClosed { get; private set; } =
        Task.FromResult(new SocketCloseResult(SocketCloseStatus.ClosedLocal, null));

    private readonly ManagedSocketOptions _options;
    private readonly Lock _locker = new();
    private Connection? _cn;
    private CancellationTokenSource _listenCts = new();

    public ClientManagedSocket(ManagedSocketOptions options, ILogger logger)
    {
        _options = options;
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
            cn.Socket.OnReceived -= HandleOnReceived;

            this.Trace("cancel listen cts");
            _listenCts.Cancel();
            _listenCts.Dispose();

            this.Trace("dispose connection");
            cn.Dispose();
        }

        this.Trace("done");
    }

    public async Task<Exception?> ConnectAsync(
        IPEndPoint endpoint,
        SslClientAuthenticationOptions? authOptions,
        CancellationToken ct = default
    )
    {
        this.Trace("start");

        // only connection is checked, because after disconnect listen task can still be awaited
        if (_cn is not null)
            throw new InvalidOperationException("Socket is already connected");

        Stream? stream = null;
        IManagedSocket? socket = null;
        try
        {
            this.Trace("create native socket");
            var nativeSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);

            this.Trace("connect native socket to {endpoint}", endpoint);
            await nativeSocket.ConnectAsync(endpoint, ct);

            if (authOptions is not null)
            {
                this.Trace("wrap native socket with network stream");
                var networkStream = new NetworkStream(nativeSocket, true);

                this.Trace("wrap network stream with ssl stream");
                var sslStream = new SslStream(
                    networkStream,
                    false,
                    authOptions.RemoteCertificateValidationCallback,
                    null
                );
                stream = sslStream;

                this.Trace("authenticate client");
                await sslStream.AuthenticateAsClientAsync(authOptions, ct);
            }
            else
            {
                this.Trace("wrap native socket with network stream");
                stream = new NetworkStream(nativeSocket, true);
            }

            this.Trace("wrap to managed socket");
            socket = Helper.GetManagedSocket(stream, _options, Logger);
            this.Trace<string, string>(
                "paired with {nativeSocket} / {managedSocket}",
                stream.GetFullId(),
                socket.GetFullId()
            );

            this.Trace("bind events");
            socket.OnReceived += HandleOnReceived;

            var cn = new Connection(stream, socket, Logger);

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
                IsClosed = socket.ListenAsync(_listenCts.Token).ContinueWith(HandleClosed, CancellationToken.None);
            }

            this.Trace("done (connected)");

            return null;
        }
        catch (Exception e)
        {
            this.Error("failed: {e}", e);

            Cleanup(stream, socket);

            this.Trace("done (not connected)");

            return e;
        }
    }

    public async Task DisconnectAsync()
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
            cn.Socket.OnReceived -= HandleOnReceived;

            this.Trace("cancel listen cts");
#pragma warning disable VSTHRD103
            _listenCts.Cancel();
            _listenCts.Dispose();

            this.Trace("dispose connection");
            cn.Dispose();
#pragma warning restore VSTHRD103
        }

        this.Trace("await listen task");
#pragma warning disable VSTHRD003
        await IsClosed;
#pragma warning restore VSTHRD003

        this.Trace("done");
    }

    public ValueTask<SocketSendStatus> SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        this.Trace("send");

        return _cn?.Socket.SendAsync(data, ct) ?? ValueTask.FromResult(SocketSendStatus.Closed);
    }

    private SocketCloseResult HandleClosed(Task<SocketCloseResult> task)
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
                this.Trace("skip - not connected");
                return task.Result;
            }

            this.Trace("unbind events");
            cn.Socket.OnReceived -= HandleOnReceived;

            this.Trace("dispose connection");
            cn.Dispose();
        }

        this.Trace("done");

        return task.Result;
#pragma warning restore VSTHRD002
    }

    private void HandleOnReceived(ReadOnlyMemory<byte> data)
    {
        this.Trace("trigger binary received");
        OnReceived(data);
    }

    private void Cleanup(Stream? stream, IManagedSocket? socket)
    {
        this.Trace("start");

        if (stream is not null)
        {
            this.Trace("dispose native socket");
            stream.Close();
        }

        if (socket is not null)
        {
            this.Trace("unbind events and dispose socket");
            socket.OnReceived -= HandleOnReceived;
            socket.Dispose();
        }

        this.Trace("done");
    }

    private sealed record Connection(Stream Stream, IManagedSocket Socket, ILogger Logger) : IDisposable, ILogSubject
    {
        public void Dispose()
        {
            try
            {
                this.Trace("dispose socket");
                Socket.Dispose();

                this.Trace("close stream");
                Stream.Close();
            }
            catch (Exception e)
            {
                this.Trace("failed: {e}", e);
            }
        }
    }
}
