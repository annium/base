using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Annium.Execution.Background;
using Annium.Logging;

namespace Annium.Extensions.Reactive.Operators;

/// <summary>
/// Provides operators for transforming observable values asynchronously in parallel
/// </summary>
public static class SelectParallelAsyncOperatorExtensions
{
    /// <summary>
    /// Projects each element of an observable sequence to a new form asynchronously in parallel
    /// </summary>
    /// <typeparam name="TSource">The type of items emitted by the source observable</typeparam>
    /// <typeparam name="TResult">The type of items emitted by the resulting observable</typeparam>
    /// <param name="source">The source observable to project</param>
    /// <param name="selector">Asynchronous function to transform each source element</param>
    /// <returns>An observable that emits the transformed values in parallel</returns>
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
                () => executor.DisposeAsync().AsTask().ContinueWith(_ => observer.OnCompleted()).GetAwaiter()
            );
        });
    }
}
