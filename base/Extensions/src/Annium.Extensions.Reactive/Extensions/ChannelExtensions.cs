using System.Threading.Channels;

// ReSharper disable once CheckNamespace
namespace System.Reactive.Linq;

/// <summary>
/// Provides extension methods for converting channels to observables
/// </summary>
public static class ChannelExtensions
{
    /// <summary>
    /// Converts a <see cref="ChannelReader{T}"/> to an <see cref="IObservable{T}"/> that emits values as they become available in the channel
    /// </summary>
    /// <typeparam name="T">The type of items in the channel</typeparam>
    /// <param name="reader">The channel reader to convert to an observable</param>
    /// <param name="onDisposed">Optional action to execute when the observable is disposed</param>
    /// <returns>An observable that emits values from the channel reader</returns>
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
