using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Net.Sockets.Internal;

/// <summary>
/// Helper class for managed socket internals: per-mode factory and shared classification helpers.
/// </summary>
internal static class Helper
{
    /// <summary>
    /// Creates a managed socket instance based on the specified mode.
    /// </summary>
    /// <param name="stream">The network stream.</param>
    /// <param name="options">Configuration options including socket mode.</param>
    /// <param name="logger">Logger instance for diagnostics.</param>
    /// <returns>A managed socket instance appropriate for the specified mode.</returns>
    /// <exception cref="InvalidOperationException">Thrown when socket mode is not supported.</exception>
    public static IManagedSocket GetManagedSocket(Stream stream, ManagedSocketOptions options, ILogger logger) =>
        options.Mode switch
        {
            SocketMode.Raw => new RawManagedSocket(stream, options, logger),
            SocketMode.Messaging => new MessagingManagedSocket(stream, options, logger),
            _ => throw new InvalidOperationException($"Unexpected socket mode {options.Mode}"),
        };

    /// <summary>
    /// Classifies an exception thrown during a send operation into a <see cref="SocketSendStatus"/>.
    /// </summary>
    /// <param name="e">The exception thrown by the send path.</param>
    /// <param name="log">Log subject for tracing.</param>
    /// <returns>The corresponding to send status.</returns>
    public static SocketSendStatus ClassifySendException(Exception e, ILogSubject log)
    {
        switch (e)
        {
            case OperationCanceledException:
                log.Trace("send canceled with OperationCanceledException");
                return SocketSendStatus.Canceled;
            // ObjectDisposedException must precede InvalidOperationException because
            // ObjectDisposedException : InvalidOperationException — the broader case would otherwise mask it.
            case ObjectDisposedException:
                log.Trace("send closed with ObjectDisposedException");
                return SocketSendStatus.Closed;
            case InvalidOperationException:
                log.Trace("send closed with InvalidOperationException: {e}", e);
                return SocketSendStatus.Closed;
            case IOException { InnerException: ObjectDisposedException }:
                log.Trace("send closed with IOException(ObjectDisposedException)");
                return SocketSendStatus.Closed;
            case IOException { InnerException: SocketException }:
                log.Trace("send closed with IOException(SocketException)");
                return SocketSendStatus.Closed;
            default:
                log.Error("send closed with {error}", e);
                return SocketSendStatus.Closed;
        }
    }

    /// <summary>
    /// Reads a single chunk from <paramref name="stream"/> into <paramref name="freeSpace"/> and classifies
    /// the result. Mirrors the per-mode receive paths so both Raw and Messaging managed sockets share the
    /// same exception handling.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="freeSpace">The buffer slice to receive into.</param>
    /// <param name="ct">Cancellation token for the read.</param>
    /// <param name="log">Log subject for tracing.</param>
    /// <returns>The reception result.</returns>
    public static async ValueTask<ReceiveResult> ReceiveChunkAsync(
        Stream stream,
        Memory<byte> freeSpace,
        CancellationToken ct,
        ILogSubject log
    )
    {
        log.Trace("start");

        try
        {
            if (ct.IsCancellationRequested)
            {
                log.Trace("canceled with cancellation token");
                return new ReceiveResult(0, SocketCloseStatus.ClosedLocal, null);
            }

            log.Trace("wait for message");
            var bytesRead = await stream.ReadAsync(freeSpace, ct).ConfigureAwait(false);
            log.Trace("received {bytesRead} bytes", bytesRead);

            return new ReceiveResult(bytesRead, bytesRead <= 0 ? SocketCloseStatus.ClosedRemote : null, null);
        }
        catch (OperationCanceledException)
        {
            log.Trace("closed locally with cancellation: {isCancellationRequested}", ct.IsCancellationRequested);
            return new ReceiveResult(0, SocketCloseStatus.ClosedLocal, null);
        }
        catch (IOException e) when (e.InnerException is ObjectDisposedException)
        {
            log.Trace("closed with ObjectDisposedException");
            return new ReceiveResult(0, SocketCloseStatus.ClosedLocal, null);
        }
        catch (IOException e) when (e.InnerException is SocketException se)
        {
            var status =
                se.SocketErrorCode is SocketError.OperationAborted
                    ? SocketCloseStatus.ClosedLocal
                    : SocketCloseStatus.ClosedRemote;
            log.Trace("{status} with SocketException (code: {code}): {e}", status, se.SocketErrorCode, se);
            return new ReceiveResult(0, status, null);
        }
        catch (Exception e)
        {
            log.Trace("Error: {e}", e);
            return new ReceiveResult(0, SocketCloseStatus.Error, e);
        }
        finally
        {
            log.Trace("done");
        }
    }

    /// <summary>
    /// Runs a managed-socket listen loop on a background <see cref="Task"/>.
    /// The supplied <paramref name="receive"/> delegate performs one receive iteration and reports whether
    /// the socket has closed.
    /// </summary>
    /// <param name="receive">Per-iteration receive delegate.</param>
    /// <param name="log">Log subject for tracing.</param>
    /// <returns>The terminal close result.</returns>
    public static Task<SocketCloseResult> RunListenLoopAsync(
        Func<ValueTask<(bool IsClosed, SocketCloseResult Result)>> receive,
        ILogSubject log
    ) =>
        Task.Run(
            async () =>
            {
                log.Trace("start");

                while (true)
                {
                    log.Trace("next");
                    var (isClosed, result) = await receive();
                    if (isClosed)
                    {
                        log.Trace(
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
}
