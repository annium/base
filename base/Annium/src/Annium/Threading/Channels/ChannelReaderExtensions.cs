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
            throw new InvalidOperationException($"Failed to write to channel {item.GetFullId()}");

        return item;
    }

    /// <summary>
    /// Pipes data from a channel reader to a channel writer with logging.
    /// </summary>
    /// <typeparam name="T">The type of items in the channel.</typeparam>
    /// <param name="reader">The source channel reader.</param>
    /// <param name="writer">The target channel writer.</param>
    /// <param name="logger">The logger to use for logging.</param>
    /// <returns>A disposable object that can be used to stop the pipe operation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDisposable Pipe<T>(this ChannelReader<T> reader, ChannelWriter<T> writer, ILogger logger)
    {
        var bridge = new LogBridge(typeof(ChannelReader<T>).FriendlyName(), logger);
        var cts = new CancellationTokenSource();
        var gate = new ManualResetEventSlim();
        Task.Run(
                async () =>
                {
                    try
                    {
                        while (await reader.WaitToReadAsync(cts.Token))
                        {
                            var data = await reader.ReadAsync(cts.Token);
                            writer.Write(data);
                        }
                    }
                    catch (OperationCanceledException) { }
                    catch (ChannelClosedException) { }
                    catch (Exception e)
                    {
                        bridge.Error(e);
                    }
                    finally
                    {
                        gate.Set();
                    }
                },
                CancellationToken.None
            )
            .GetAwaiter();

        return Disposable.Create(() =>
        {
            bridge.Trace("cancel");
            cts.Cancel();
            bridge.Trace("wait");
            gate.Wait(CancellationToken.None);
            bridge.Trace("dispose");
            cts.Dispose();
            gate.Dispose();
            bridge.Trace("done");
        });
    }

    /// <summary>
    /// Waits asynchronously until the channel reader is empty.
    /// </summary>
    /// <typeparam name="T">The type of items in the channel.</typeparam>
    /// <param name="reader">The channel reader.</param>
    /// <param name="delay">The delay in milliseconds between checks.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task WhenEmptyAsync<T>(this ChannelReader<T> reader, int delay = 25)
    {
        while (reader.TryPeek(out _))
            await Task.Delay(delay);
    }
}
