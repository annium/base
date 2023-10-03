using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Net.Sockets.Internal;

internal class ManagedSocket : ISendingReceivingSocket, ILogSubject
{
    public ILogger Logger { get; }
    public event Action<ReadOnlyMemory<byte>> OnReceived = delegate { };
    private const int BufferSize = 65_536;
    private readonly Stream _stream;

    public ManagedSocket(Stream socket, ILogger logger)
    {
        Logger = logger;
        _stream = socket;
    }

    public async ValueTask<SocketSendStatus> SendAsync(ReadOnlyMemory<byte> data, CancellationToken ct = default)
    {
        try
        {
            this.Trace("{dataLength} - start", data.Length);

            if (ct.IsCancellationRequested)
            {
                this.Trace("{dataLength} - canceled with cancellation token", data.Length);
                return SocketSendStatus.Canceled;
            }

            await _stream.WriteAsync(data, ct).ConfigureAwait(false);
            this.Trace("{dataLength} - send succeed", data.Length);

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
    }

    public async Task<SocketCloseResult> ListenAsync(CancellationToken ct)
    {
        using var buffer = new DynamicBuffer<byte>(BufferSize);

        this.Trace("start");

        while (true)
        {
            var (isClosed, result) = await ReceiveAsync(buffer, ct);
            if (isClosed)
            {
                this.Trace(result.Exception is not null ? $"stop with {result.Status}: {result.Exception}" : $"stop with {result.Status}");
                return result;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private async ValueTask<(bool IsClosed, SocketCloseResult Result)> ReceiveAsync(DynamicBuffer<byte> buffer, CancellationToken ct)
    {
        // reset buffer to start writing from start
        buffer.Reset();

        while (true)
        {
            // read chunk into buffer
            var receiveResult = await ReceiveChunkAsync(buffer, ct).ConfigureAwait(false);

            // if close received - return false, indicating socket is closed
            if (receiveResult.Status.HasValue)
            {
                return (true, new SocketCloseResult(receiveResult.Status.Value, receiveResult.Exception));
            }

            // track receiveResult count
            buffer.TrackDataSize(receiveResult.Count);

            this.Trace("fire message received");
            OnReceived(buffer.AsDataReadOnlyMemory());

            return (false, new SocketCloseResult(SocketCloseStatus.ClosedRemote, null));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private async ValueTask<ReceiveResult> ReceiveChunkAsync(DynamicBuffer<byte> buffer, CancellationToken ct)
    {
        try
        {
            if (ct.IsCancellationRequested)
            {
                this.Trace("canceled with cancellation token");
                return new ReceiveResult(0, SocketCloseStatus.ClosedLocal, null);
            }

            var bytesRead = await _stream.ReadAsync(buffer.AsFreeSpaceMemory(), ct).ConfigureAwait(false);
            this.Trace("received {bytesRead} bytes", bytesRead);

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
            var status = se.SocketErrorCode is SocketError.OperationAborted
                ? SocketCloseStatus.ClosedLocal
                : SocketCloseStatus.ClosedRemote;
            this.Trace("{status} with SocketException (code: {code}): {e}", status, se.SocketErrorCode, se);
            return new ReceiveResult(0, status, null);
        }
        catch (Exception e)
        {
            this.Trace("Error!!: {e}", e);
            return new ReceiveResult(0, SocketCloseStatus.Error, e);
        }
    }
}