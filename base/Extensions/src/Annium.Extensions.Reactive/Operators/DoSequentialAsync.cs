using System.Reactive.Disposables;
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
            var teardown = new ExecutorTeardown<T>(executor, observer);
            var subscription = source.Subscribe(
                x =>
                    executor.Schedule(async () =>
                    {
                        // work that has not started yet can skip the handler once the sequence has
                        // failed - on the parallel executor the items already in flight still finish
                        if (teardown.HasFailed)
                            return;

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
                            teardown.Fail(e);
                        }
                    }),
                // without an onError the source's failure had nowhere to go: the downstream
                // observer never heard of it and the executor was left running
                teardown.Fail,
                teardown.Complete
            );

            // the source subscription alone would leave the executor's background loop running for a
            // subscriber that disposed before the source ever ended
            return Disposable.Create(() =>
            {
                subscription.Dispose();
                teardown.Cancel();
            });
        });
    }
}
