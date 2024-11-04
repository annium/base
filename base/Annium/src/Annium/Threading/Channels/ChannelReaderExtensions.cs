using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Threading.Channels;

public static class ChannelReaderExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Read<T>(this ChannelReader<T> reader)
    {
        if (!reader.TryRead(out var item))
            throw new InvalidOperationException($"Failed to write to channel {item.GetFullId()}");

        return item;
    }

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
                        while (!cts.IsCancellationRequested)
                        {
                            while (await reader.WaitToReadAsync(cts.Token))
                            {
                                var data = await reader.ReadAsync(cts.Token);
                                writer.Write(data);
                            }
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task WhenEmptyAsync<T>(this ChannelReader<T> reader, int delay = 25)
    {
        while (reader.TryPeek(out _))
            await Task.Delay(delay);
    }
}
