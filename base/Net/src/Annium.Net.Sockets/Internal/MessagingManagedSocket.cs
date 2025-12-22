using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Net.Sockets.Internal;

/// <summary>
/// A managed socket implementation that handles framed messaging over a stream with length prefixes
/// </summary>
internal class MessagingManagedSocket : IManagedSocket, ILogSubject
{
    /// <summary>
    /// Logger for tracing socket operations
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Event raised when data is received from the socket
    /// </summary>
    public event Action<ReadOnlyMemory<byte>> OnReceived = delegate { };

    /// <summary>
    /// The underlying stream for socket communication
    /// </summary>
    private readonly Stream _stream;

    /// <summary>
    /// Configuration options for the managed socket
    /// </summary>
    private readonly ManagedSocketOptionsBase _options;

    /// <summary>
    /// Semaphore to ensure thread-safe sending operations
    /// </summary>
    private readonly SemaphoreSlim _gate = new(1, 1);

    /// <summary>
    /// Flag indicating whether the socket has been disposed
    /// </summary>
    private bool _isDisposed;

    /// <summary>
    /// Counter tracking total bytes sent
    /// </summary>
    private long _sendCounter;

    /// <summary>
    /// Counter tracking total bytes received
    /// </summary>
    private long _recvCounter;

    /// <summary>
    /// Initializes a new instance of the MessagingManagedSocket class
    /// </summary>
    /// <param name="stream">The underlying stream for socket communication</param>
    /// <param name="options">Configuration options for the managed socket</param>
    /// <param name="logger">Logger for tracing socket operations</param>
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
    /// Sends data through the socket with a length prefix frame
    /// </summary>
    /// <param name="data">The data to send</param>
    /// <param name="ct">Cancellation token for the operation</param>
    /// <returns>The status of the send operation</returns>
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

        try
        {
            await _gate.WaitAsync(ct);

            var messageSize = BitConverter.GetBytes(data.Length);

            this.Trace(
                "{dataLength} - send message size (total: {total})",
                data.Length,
                _sendCounter += messageSize.Length
            );
            await _stream.WriteAsync(messageSize, ct).ConfigureAwait(false);

            this.Trace("{dataLength} - message itself (total: {total})", data.Length, _sendCounter += data.Length);
            await _stream.WriteAsync(data, ct).ConfigureAwait(false);

            this.Trace("{dataLength} - send succeed (total: {total})", data.Length, _sendCounter);

            return SocketSendStatus.Ok;
        }
        catch (OperationCanceledException)
        {
            this.Trace("{dataLength} - canceled with OperationCanceledException", data.Length);
            return SocketSendStatus.Canceled;
        }
        catch (InvalidOperationException e)
        {
            this.Trace("{dataLength} - closed with InvalidOperationException: {e}", data.Length, e);
            return SocketSendStatus.Closed;
        }
        catch (IOException e) when (e.InnerException is ObjectDisposedException)
        {
            this.Trace("{dataLength} - closed with IOException(ObjectDisposedException)", data.Length);
            return SocketSendStatus.Closed;
        }
        catch (IOException e) when (e.InnerException is SocketException)
        {
            this.Trace("{dataLength} - closed with IOException(SocketException)", data.Length);
            return SocketSendStatus.Closed;
        }
        catch (Exception e)
        {
            this.Error("{dataLength} - closed with {error}", data.Length, e);
            return SocketSendStatus.Closed;
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>
    /// Starts listening for incoming messages on the socket
    /// </summary>
    /// <param name="ct">Cancellation token for the listening operation</param>
    /// <returns>The result of the socket close operation when listening ends</returns>
    public Task<SocketCloseResult> ListenAsync(CancellationToken ct) =>
        Task.Run(
            async () =>
            {
                if (_isDisposed)
                {
                    this.Trace("disposed, return closed local");
                    return new SocketCloseResult(SocketCloseStatus.ClosedLocal, null);
                }

                using var buffer = new MessagingBuffer(_options.BufferSize, _options.ExtremeMessageSize);

                this.Trace("start");

                while (true)
                {
                    this.Trace("next");
                    var (isClosed, result) = await ReceiveAsync(buffer, ct);
                    if (isClosed)
                    {
                        this.Trace(
                            result.Exception is not null
                                ? $"stop with {result.Status}: {result.Exception}"
                                : $"stop with {result.Status}"
                        );
                        return result;
                    }
                }
            },
            CancellationToken.None
        );

    /// <summary>
    /// Disposes the managed socket and releases associated resources
    /// </summary>
    public void Dispose()
    {
        this.Trace("start");

        if (_isDisposed)
        {
            this.Trace("already disposed");
            return;
        }

        _isDisposed = true;

        _gate.Dispose();

        this.Trace("done");
    }

    /// <summary>
    /// Receives data from the stream into the buffer and processes complete messages
    /// </summary>
    /// <param name="buffer">The buffer to receive data into</param>
    /// <param name="ct">Cancellation token for the operation</param>
    /// <returns>A tuple indicating if the socket is closed and the close result</returns>
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
        var receiveResult = await ReceiveChunkAsync(buffer, ct).ConfigureAwait(false);

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

    /// <summary>
    /// Receives a chunk of data from the stream into the buffer
    /// </summary>
    /// <param name="buffer">The buffer to receive data into</param>
    /// <param name="ct">Cancellation token for the operation</param>
    /// <returns>The result of the receive operation</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private async ValueTask<ReceiveResult> ReceiveChunkAsync(MessagingBuffer buffer, CancellationToken ct)
    {
        this.Trace("start");

        try
        {
            if (ct.IsCancellationRequested)
            {
                this.Trace("canceled with cancellation token");
                return new ReceiveResult(0, SocketCloseStatus.ClosedLocal, null);
            }

            this.Trace("wait for message");
            var bytesRead = await _stream.ReadAsync(buffer.FreeSpace, ct).ConfigureAwait(false);
            this.Trace("received {bytesRead} bytes (total: {total})", bytesRead, _recvCounter += bytesRead);

            return new ReceiveResult(bytesRead, bytesRead <= 0 ? SocketCloseStatus.ClosedRemote : null, null);
        }
        catch (OperationCanceledException)
        {
            this.Trace("closed locally with cancellation: {isCancellationRequested}", ct.IsCancellationRequested);
            return new ReceiveResult(0, SocketCloseStatus.ClosedLocal, null);
        }
        catch (IOException e) when (e.InnerException is ObjectDisposedException)
        {
            this.Trace("closed with ObjectDisposedException");
            return new ReceiveResult(0, SocketCloseStatus.ClosedLocal, null);
        }
        catch (IOException e) when (e.InnerException is SocketException se)
        {
            var status =
                se.SocketErrorCode is SocketError.OperationAborted
                    ? SocketCloseStatus.ClosedLocal
                    : SocketCloseStatus.ClosedRemote;
            this.Trace("{status} with SocketException (code: {code}): {e}", status, se.SocketErrorCode, se);
            return new ReceiveResult(0, status, null);
        }
        catch (Exception e)
        {
            this.Trace("Error: {e}", e);
            return new ReceiveResult(0, SocketCloseStatus.Error, e);
        }
        finally
        {
            this.Trace("done");
        }
    }
}
