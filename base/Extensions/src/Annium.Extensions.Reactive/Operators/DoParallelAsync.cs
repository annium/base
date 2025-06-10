using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Annium.Execution.Background;
using Annium.Logging;

namespace Annium.Extensions.Reactive.Operators;

/// <summary>
/// Provides operators for executing side effects asynchronously in parallel
/// </summary>
public static class DoParallelAsyncOperatorExtensions
{
    /// <summary>
    /// Performs an asynchronous side effect on each emitted value in parallel without blocking the observable sequence
    /// </summary>
    /// <typeparam name="T">The type of items emitted by the source observable</typeparam>
    /// <param name="source">The source observable</param>
    /// <param name="handle">Asynchronous function to execute as a side effect for each value</param>
    /// <returns>An observable that emits the same values as the source after the side effect has been scheduled</returns>
    public static IObservable<T> DoParallelAsync<T>(this IObservable<T> source, Func<T, Task> handle)
    {
        return Observable.Create<T>(observer =>
        {
            var executor = Executor.Parallel<IObservable<T>>(VoidLogger.Instance).Start();
            return source.Subscribe(
                x =>
                    executor.Schedule(async () =>
                    {
                        await handle(x);
                        observer.OnNext(x);
                    }),
                () => executor.DisposeAsync().AsTask().ContinueWith(_ => observer.OnCompleted()).GetAwaiter()
            );
        });
    }
}
