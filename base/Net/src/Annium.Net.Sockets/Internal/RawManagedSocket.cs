using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Net.Sockets.Internal;

/// <summary>
/// Implementation of a managed socket for raw data transmission without message framing
/// </summary>
internal class RawManagedSocket : IManagedSocket, ILogSubject
{
    /// <summary>
    /// Gets the logger instance
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Event raised when data is received
    /// </summary>
    public event Action<ReadOnlyMemory<byte>> OnReceived = delegate { };

    /// <summary>
    /// The underlying network stream
    /// </summary>
    private readonly Stream _stream;

    /// <summary>
    /// Configuration options for the socket
    /// </summary>
    private readonly ManagedSocketOptionsBase _options;

    /// <summary>
    /// Counter for bytes sent
    /// </summary>
    private long _sendCounter;

    /// <summary>
    /// Counter for bytes received
    /// </summary>
    private long _recvCounter;

    /// <summary>
    /// Initializes a new instance of the RawManagedSocket class
    /// </summary>
    /// <param name="socket">The network stream</param>
    /// <param name="options">Configuration options</param>
    /// <param name="logger">Logger instance for diagnostics</param>
    public RawManagedSocket(Stream socket, ManagedSocketOptionsBase options, ILogger logger)
    {
        Logger = logger;
        _stream = socket;
        _options = options;
        this.Trace("buffer size: {bufferSize}", options.BufferSize);
    }

    /// <summary>
    /// Sends data asynchronously
    /// </summary>
    /// <param name="data">The data to send</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The status of the send operation</returns>
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
            this.Trace("{dataLength} - send succeed (total: {total})", data.Length, _sendCounter += data.Length);

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
    }

    /// <summary>
    /// Starts listening for incoming data asynchronously
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A task that completes with the socket close result when listening ends</returns>
    public Task<SocketCloseResult> ListenAsync(CancellationToken ct) =>
        Task.Run(
            async () =>
            {
                using var buffer = new RawBuffer(_options.BufferSize);

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
    /// Disposes the socket resources
    /// </summary>
    public void Dispose()
    {
        this.Trace("run");
    }

    /// <summary>
    /// Receives data into the buffer asynchronously
    /// </summary>
    /// <param name="buffer">The buffer to receive data into</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A tuple indicating if the socket is closed and the close result</returns>
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
        var receiveResult = await ReceiveChunkAsync(buffer, ct).ConfigureAwait(false);

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

    /// <summary>
    /// Receives a chunk of data from the stream
    /// </summary>
    /// <param name="buffer">The buffer to receive data into</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The result of the receive operation</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private async ValueTask<ReceiveResult> ReceiveChunkAsync(RawBuffer buffer, CancellationToken ct)
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
