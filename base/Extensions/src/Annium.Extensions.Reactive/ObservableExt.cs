using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Annium.Extensions.Reactive.Internal.Creation.Instance;
using Annium.Logging;

// ReSharper disable once CheckNamespace
namespace System.Reactive.Linq;

public static class ObservableExt
{
    #region Instance

    // public static IObservableInstance<T> Dynamic<T>(Func<ObserverContext<T>, Task<Func<Task>>> factory)
    // {
    //     return new DynamicObservableInstance<T>(factory);
    // }

    public static IObservable<T> StaticAsyncInstance<T>(
        Func<ObserverContext<T>, Task<Func<Task>>> factory,
        CancellationToken ct,
        ILogger logger
    )
    {
        return new StaticObservableInstance<T>(factory, true, ct, logger);
    }

    public static IObservable<T> StaticSyncInstance<T>(
        Func<ObserverContext<T>, Task<Func<Task>>> factory,
        CancellationToken ct,
        ILogger logger
    )
    {
        return new StaticObservableInstance<T>(factory, false, ct, logger);
    }

    #endregion

    #region From

    public static IObservable<T> FromChannel<T>(
        ChannelReader<T> reader,
        Action? onDisposed = null
    )
    {
        return Observable.Create<T>(async (observer, ct) =>
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    var data = await reader.ReadAsync(ct);
                    observer.OnNext(data);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception)
            {
                //
            }

            return onDisposed ?? OnDisposed;
        });

        static void OnDisposed()
        {
        }
    }

    #endregion
}