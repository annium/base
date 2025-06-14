using System.Reactive.Linq;
using System.Threading.Tasks;
using Annium.Execution.Background;
using Annium.Logging;

// ReSharper disable once CheckNamespace
namespace System;

/// <summary>
/// Provides operators for transforming observable values asynchronously in sequential order
/// </summary>
public static class SelectSequentialAsyncOperatorExtensions
{
    /// <summary>
    /// Projects each element of an observable sequence to a new form asynchronously in sequential order
    /// </summary>
    /// <typeparam name="TSource">The type of items emitted by the source observable</typeparam>
    /// <typeparam name="TResult">The type of items emitted by the resulting observable</typeparam>
    /// <param name="source">The source observable to project</param>
    /// <param name="selector">Asynchronous function to transform each source element</param>
    /// <returns>An observable that emits the transformed values sequentially</returns>
    public static IObservable<TResult> SelectSequentialAsync<TSource, TResult>(
        this IObservable<TSource> source,
        Func<TSource, Task<TResult>> selector
    )
    {
        return Observable.Create<TResult>(observer =>
        {
            var executor = Executor.Sequential<IObservable<TSource>>(VoidLogger.Instance).Start();
            return source.Subscribe(
                x =>
                    executor.Schedule(async () =>
                    {
                        observer.OnNext(await selector(x));
                    }),
                () => executor.DisposeAsync().AsTask().ContinueWith(_ => observer.OnCompleted()).GetAwaiter()
            );
        });
    }
}
