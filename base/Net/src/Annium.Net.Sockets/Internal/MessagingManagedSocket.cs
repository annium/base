using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Net.Sockets.Internal;

internal class MessagingManagedSocket : IManagedSocket, ILogSubject
{
    public ILogger Logger { get; }
    public event Action<ReadOnlyMemory<byte>> OnReceived = delegate { };
    private readonly Stream _stream;
    private readonly ManagedSocketOptionsBase _options;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private bool _isDisposed;
    private long _sendCounter;
    private long _recvCounter;

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
        finally
        {
            _gate.Release();
        }
    }

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
        GC.SuppressFinalize(this);

        this.Trace("done");
    }

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
