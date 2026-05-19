using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Threading.Channels;

/// <summary>
/// Provides extension methods for working with channel readers.
/// </summary>
public static class ChannelReaderExtensions
{
    /// <summary>
    /// Default poll cadence in milliseconds for <see cref="WhenEmptyAsync{T}"/>. Matches the
    /// same default used by <c>Wait</c> and <c>Expect</c> so behaviour stays consistent across the
    /// area's polling primitives.
    /// </summary>
    private const int DefaultDelayMs = 25;

    /// <summary>
    /// Reads an item from the channel reader.
    /// </summary>
    /// <typeparam name="T">The type of items in the channel.</typeparam>
    /// <param name="reader">The channel reader.</param>
    /// <returns>The read item.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the read operation fails.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Read<T>(this ChannelReader<T> reader)
    {
        if (!reader.TryRead(out var item))
            throw new InvalidOperationException("Failed to read from channel");

        return item;
    }

    /// <summary>
    /// Pipes data from a channel reader to a channel writer with logging.
    /// </summary>
    /// <typeparam name="T">The type of items in the channel.</typeparam>
    /// <param name="reader">The source channel reader.</param>
    /// <param name="writer">The target channel writer.</param>
    /// <param name="logger">The logger to use for logging.</param>
    /// <returns>An asynchronous disposable that, when disposed, cancels the background pipe loop and awaits its completion.</returns>
    public static IAsyncDisposable Pipe<T>(this ChannelReader<T> reader, ChannelWriter<T> writer, ILogger logger)
    {
        var bridge = new LogBridge(typeof(ChannelReader<T>).FriendlyName(), logger);
        var cts = new CancellationTokenSource();
        var loop = Task.Run(
            async () =>
            {
                try
                {
                    while (await reader.WaitToReadAsync(cts.Token).ConfigureAwait(false))
                    {
                        var data = await reader.ReadAsync(cts.Token).ConfigureAwait(false);
                        writer.Write(data);
                    }
                }
                catch (OperationCanceledException) { }
                catch (ChannelClosedException) { }
                catch (Exception e)
                {
                    bridge.Error(e);
                }
            },
            CancellationToken.None
        );

        return Disposable.Create(async () =>
        {
            bridge.Trace("cancel");
            await cts.CancelAsync().ConfigureAwait(false);
            bridge.Trace("await loop");
            // VSTHRD003: awaiting loop Task during dispose teardown — intentional; CTS is cancelled before this point.
#pragma warning disable VSTHRD003
            await loop.ConfigureAwait(false);
#pragma warning restore VSTHRD003
            bridge.Trace("dispose");
            // VSTHRD103: CancellationTokenSource.Dispose() has no async overload; synchronous call after await is correct.
#pragma warning disable VSTHRD103
            cts.Dispose();
#pragma warning restore VSTHRD103
            bridge.Trace("done");
        });
    }

    /// <summary>
    /// Waits asynchronously until the channel reader is empty.
    /// </summary>
    /// <typeparam name="T">The type of items in the channel.</typeparam>
    /// <param name="reader">The channel reader.</param>
    /// <param name="delay">The delay in milliseconds between checks.</param>
    /// <param name="ct">A cancellation token that aborts the wait.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task WhenEmptyAsync<T>(
        this ChannelReader<T> reader,
        int delay = DefaultDelayMs,
        CancellationToken ct = default
    )
    {
        while (reader.TryPeek(out _))
            await Task.Delay(delay, ct).ConfigureAwait(false);
    }
}
