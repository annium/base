using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Net.Sockets.Internal;

/// <summary>
/// A managed socket implementation that handles framed messaging over a stream with length prefixes.
/// </summary>
internal class MessagingManagedSocket : IManagedSocket, ILogSubject
{
    /// <summary>
    /// Logger for tracing socket operations.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Event raised when data is received from the socket.
    /// </summary>
    public event Action<ReadOnlyMemory<byte>> OnReceived = delegate { };

    /// <summary>
    /// The underlying stream for socket communication.
    /// </summary>
    private readonly Stream _stream;

    /// <summary>
    /// Configuration options for the managed socket.
    /// </summary>
    private readonly ManagedSocketOptionsBase _options;

    /// <summary>
    /// Semaphore to ensure thread-safe sending operations.
    /// </summary>
    private readonly SemaphoreSlim _gate = new(1, 1);

    /// <summary>
    /// Flag indicating whether the socket has been disposed. Volatile so a concurrent
    /// SendAsync observes the write set by Dispose() before attempting to acquire _gate.
    /// </summary>
    private volatile bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the MessagingManagedSocket class.
    /// </summary>
    /// <param name="stream">The underlying stream for socket communication.</param>
    /// <param name="options">Configuration options for the managed socket.</param>
    /// <param name="logger">Logger for tracing socket operations.</param>
    public MessagingManagedSocket(Stream stream, ManagedSocketOptionsBase options, ILogger logger)
    {
        Logger = logger;
        _stream = stream;
        _options = options;
        this.Trace(
            "buffer size: {bufferSize}, extreme message size: {extremeMessageSize}",
            options.BufferSize,
            options.ExtremeMessageSize
        );
    }

    /// <summary>
    /// Sends data through the socket with a length prefix frame.
    /// </summary>
    /// <param name="data">The data to send.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>The status of the send operation.</returns>
    public async ValueTask<SocketSendStatus> SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        this.Trace("{dataLength} - start", data.Length);

        if (_isDisposed)
        {
            this.Trace("{dataLength} - disposed, return closed", data.Length);
            return SocketSendStatus.Closed;
        }

        if (ct.IsCancellationRequested)
        {
            this.Trace("{dataLength} - canceled with cancellation token", data.Length);
            return SocketSendStatus.Canceled;
        }

        var acquired = false;
        try
        {
            try
            {
                await _gate.WaitAsync(ct);
                acquired = true;
            }
            catch (ObjectDisposedException)
            {
                // Dispose() raced ahead and disposed _gate after we passed the _isDisposed check.
                this.Trace("{dataLength} - disposed during gate acquisition, return closed", data.Length);
                return SocketSendStatus.Closed;
            }

            var messageSize = BitConverter.GetBytes(data.Length);

            this.Trace("{dataLength} - send message size", data.Length);
            await _stream.WriteAsync(messageSize, ct).ConfigureAwait(false);

            this.Trace("{dataLength} - message itself", data.Length);
            await _stream.WriteAsync(data, ct).ConfigureAwait(false);

            this.Trace("{dataLength} - send succeed", data.Length);

            return SocketSendStatus.Ok;
        }
        catch (Exception e)
        {
            return Helper.ClassifySendException(e, this);
        }
        finally
        {
            if (acquired)
                _gate.Release();
        }
    }

    /// <summary>
    /// Starts listening for incoming messages on the socket.
    /// </summary>
    /// <param name="ct">Cancellation token for the listening operation.</param>
    /// <returns>The result of the socket close operation when listening ends.</returns>
    public async Task<SocketCloseResult> ListenAsync(CancellationToken ct)
    {
        if (_isDisposed)
        {
            this.Trace("disposed, return closed local");
            return new SocketCloseResult(SocketCloseStatus.ClosedLocal, null);
        }

        using var buffer = new MessagingBuffer(_options.BufferSize, _options.ExtremeMessageSize);
        return await Helper.RunListenLoopAsync(() => ReceiveAsync(buffer, ct), this);
    }

    /// <summary>
    /// Disposes the managed socket and releases associated resources.
    /// </summary>
    public void Dispose()
    {
        this.Trace("start");

        if (_isDisposed)
        {
            this.Trace("already disposed");
            return;
        }

        // mark disposed BEFORE releasing _gate so a concurrent SendAsync that just passed
        // the _isDisposed check observes the write the next time it reads the field. The
        // remaining race (SendAsync calls _gate.WaitAsync on a disposed semaphore) is handled
        // by the inner ObjectDisposedException catch in SendAsync.
        _isDisposed = true;

        _gate.Dispose();

        this.Trace("done");
    }

    /// <summary>
    /// Receives data from the stream into the buffer and processes complete messages.
    /// </summary>
    /// <param name="buffer">The buffer to receive data into.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>A tuple indicating if the socket is closed and the close result.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private async ValueTask<(bool IsClosed, SocketCloseResult Result)> ReceiveAsync(
        MessagingBuffer buffer,
        CancellationToken ct
    )
    {
        this.Trace("start");

        // grow buffer if needed
        if (buffer.IsFull)
        {
            this.Trace("buffer {buffer} is full, grow", buffer);
            buffer.Grow();
        }

        // read chunk into buffer
        this.Trace("receive data chunk into buffer {buffer}", buffer);
        var receiveResult = await Helper.ReceiveChunkAsync(_stream, buffer.FreeSpace, ct, this).ConfigureAwait(false);

        // if close received - return false, indicating socket is closed
        if (receiveResult.Status.HasValue)
        {
            this.Trace("closed with {status}", receiveResult.Status.Value);
            return (true, new SocketCloseResult(receiveResult.Status.Value, receiveResult.Exception));
        }

        // track receiveResult count
        this.Trace("track received data size: {size}", receiveResult.Count);
        buffer.TrackData(receiveResult.Count);

        while (true)
        {
            if (buffer.HasInvalidHeader)
            {
                this.Trace("buffer {buffer} reported invalid (negative) header length, close with error", buffer);
                return (
                    true,
                    new SocketCloseResult(
                        SocketCloseStatus.Error,
                        new InvalidDataException("Negative message length in frame header")
                    )
                );
            }

            if (buffer.ExtremeMessageExpected)
            {
                this.Trace("buffer {buffer} has extreme message expected, close with error", buffer);
                return (
                    true,
                    new SocketCloseResult(SocketCloseStatus.Error, new Exception("Extreme message expected in buffer"))
                );
            }

            if (!buffer.ContainsFullMessage)
                break;

            this.Trace("buffer {buffer} contains full message, fire message received", buffer);
            OnReceived(buffer.Message);

            // reset buffer to forget fired message
            this.Trace("reset buffer");
            buffer.Reset();
        }

        this.Trace("buffer {buffer} doesn't contain full message, done", buffer);

        return (false, new SocketCloseResult(SocketCloseStatus.ClosedRemote, null));
    }
}
