using System.Threading.Channels;

// ReSharper disable once CheckNamespace
namespace System.Reactive.Linq;

public static class ChannelExtensions
{
    public static IObservable<T> AsObservable<T>(this ChannelReader<T> reader, Action? onDisposed = null)
    {
        return Observable.Create<T>(
            async (observer, ct) =>
            {
                try
                {
                    while (!ct.IsCancellationRequested)
                    {
                        while (await reader.WaitToReadAsync(ct))
                        {
                            var data = await reader.ReadAsync(ct);
                            observer.OnNext(data);
                        }
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception)
                {
                    //
                }

                return onDisposed ?? Disposed;
            }
        );

        static void Disposed() { }
    }
}
