using System.Reactive.Linq;
using System.Threading.Tasks;
using Annium.Execution.Background;
using Annium.Extensions.Reactive.Internal;
using Annium.Logging;

// ReSharper disable once CheckNamespace
namespace System;

/// <summary>
/// Provides operators for executing side effects asynchronously in sequential order
/// </summary>
public static class DoSequentialAsyncOperatorExtensions
{
    /// <summary>
    /// Performs an asynchronous side effect on each emitted value sequentially without blocking the observable sequence
    /// </summary>
    /// <typeparam name="T">The type of items emitted by the source observable</typeparam>
    /// <param name="source">The source observable</param>
    /// <param name="handle">Asynchronous function to execute as a side effect for each value</param>
    /// <returns>An observable that emits the same values as the source after the side effect has been scheduled</returns>
    public static IObservable<T> DoSequentialAsync<T>(this IObservable<T> source, Func<T, Task> handle)
    {
        return Observable.Create<T>(observer =>
        {
            var executor = Executor.Sequential<IObservable<T>>(VoidLogger.Instance).Start();
            return source.Subscribe(
                x =>
                    executor.Schedule(async () =>
                    {
                        await handle(x);
                        observer.OnNext(x);
                    }),
                // without an onError the source's failure had nowhere to go: the downstream
                // observer never heard of it and the executor was left running
                e => ExecutorTeardown.FailInBackground(executor, observer, e),
                () => ExecutorTeardown.CompleteInBackground(executor, observer)
            );
        });
    }
}
