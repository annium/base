using System.Reactive.Linq;
using System.Threading.Tasks;
using Annium.Execution.Background;
using Annium.Extensions.Reactive.Internal;
using Annium.Logging;

// ReSharper disable once CheckNamespace
namespace System;

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
                        try
                        {
                            await handle(x);
                            observer.OnNext(x);
                        }
                        catch (Exception e)
                        {
                            // the executor logs into a VoidLogger, so an exception from the caller's
                            // own handler is discarded there - the item vanishes and the sequence
                            // carries on as if nothing happened. Forwarding it ends the sequence, as a
                            // throwing selector does in Rx's own Select
                            ExecutorTeardown.FailInBackground(executor, observer, e);
                        }
                    }),
                // without an onError the source's failure had nowhere to go: the downstream
                // observer never heard of it and the executor was left running
                e => ExecutorTeardown.FailInBackground(executor, observer, e),
                () => ExecutorTeardown.CompleteInBackground(executor, observer)
            );
        });
    }
}
