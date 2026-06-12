using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Net.Sockets.Internal;

/// <summary>
/// Implementation of a managed socket for raw data transmission without message framing.
/// </summary>
internal class RawManagedSocket : IManagedSocket, ILogSubject
{
    /// <summary>
    /// Gets the logger instance.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Event raised when data is received.
    /// </summary>
    public event Action<ReadOnlyMemory<byte>> OnReceived = delegate { };

    /// <summary>
    /// The underlying network stream.
    /// </summary>
    private readonly Stream _stream;

    /// <summary>
    /// Configuration options for the socket.
    /// </summary>
    private readonly ManagedSocketOptionsBase _options;

    /// <summary>
    /// Initializes a new instance of the RawManagedSocket class.
    /// </summary>
    /// <param name="stream">The network stream.</param>
    /// <param name="options">Configuration options.</param>
    /// <param name="logger">Logger instance for diagnostics.</param>
    public RawManagedSocket(Stream stream, ManagedSocketOptionsBase options, ILogger logger)
    {
        Logger = logger;
        _stream = stream;
        _options = options;
        this.Trace("buffer size: {bufferSize}", options.BufferSize);
    }

    /// <summary>
    /// Sends data asynchronously.
    /// </summary>
    /// <param name="data">The data to send.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The status of the send operation.</returns>
    public async ValueTask<SocketSendStatus> SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        this.Trace("{dataLength} - start", data.Length);

        if (ct.IsCancellationRequested)
        {
            this.Trace("{dataLength} - canceled with cancellation token", data.Length);
            return SocketSendStatus.Canceled;
        }

        try
        {
            await _stream.WriteAsync(data, ct).ConfigureAwait(false);
            this.Trace("{dataLength} - send succeed", data.Length);

            return SocketSendStatus.Ok;
        }
        catch (Exception e)
        {
            return Helper.ClassifySendException(e, this);
        }
    }

    /// <summary>
    /// Starts listening for incoming data asynchronously.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that completes with the socket close result when listening ends.</returns>
    public async Task<SocketCloseResult> ListenAsync(CancellationToken ct)
    {
        using var buffer = new RawBuffer(_options.BufferSize);
        return await Helper.RunListenLoopAsync(() => ReceiveAsync(buffer, ct), this);
    }

    /// <summary>
    /// Disposes the socket resources.
    /// </summary>
    public void Dispose()
    {
        this.Trace("run");
    }

    /// <summary>
    /// Receives data into the buffer asynchronously.
    /// </summary>
    /// <param name="buffer">The buffer to receive data into.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A tuple indicating if the socket is closed and the close result.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private async ValueTask<(bool IsClosed, SocketCloseResult Result)> ReceiveAsync(
        RawBuffer buffer,
        CancellationToken ct
    )
    {
        this.Trace("start");

        // reset buffer to start writing from start
        this.Trace("reset buffer");
        buffer.Reset();

        // read chunk into buffer
        this.Trace("receive chunk");
        var receiveResult = await Helper.ReceiveChunkAsync(_stream, buffer.FreeSpace, ct, this).ConfigureAwait(false);

        // if close received - return false, indicating socket is closed
        if (receiveResult.Status.HasValue)
        {
            this.Trace("closed with {status}", receiveResult.Status.Value);
            return (true, new SocketCloseResult(receiveResult.Status.Value, receiveResult.Exception));
        }

        // track receiveResult count
        this.Trace("track data size: {size}", receiveResult.Count);
        buffer.TrackData(receiveResult.Count);

        this.Trace("fire message received");
        OnReceived(buffer.Data);

        this.Trace("done");

        return (false, new SocketCloseResult(SocketCloseStatus.ClosedRemote, null));
    }
}
