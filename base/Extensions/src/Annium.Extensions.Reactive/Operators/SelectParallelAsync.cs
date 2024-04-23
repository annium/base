using System.Reactive.Linq;
using System.Threading.Tasks;
using Annium.Execution.Background;
using Annium.Logging;

// ReSharper disable once CheckNamespace
namespace System;

public static class SelectParallelAsyncOperatorExtensions
{
    public static IObservable<TResult> SelectParallelAsync<TSource, TResult>(
        this IObservable<TSource> source,
        Func<TSource, Task<TResult>> selector
    )
    {
        return Observable.Create<TResult>(observer =>
        {
            var executor = Executor.Parallel<IObservable<TSource>>(VoidLogger.Instance).Start();
            return source.Subscribe(
                x =>
                    executor.Schedule(async () =>
                    {
                        observer.OnNext(await selector(x));
                    }),
                () => executor.DisposeAsync().AsTask().ContinueWith(_ => observer.OnCompleted())
            );
        });
    }
}
