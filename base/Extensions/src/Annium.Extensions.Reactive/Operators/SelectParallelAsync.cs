using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Annium.Execution.Background;
using Annium.Extensions.Reactive.Internal;
using Annium.Logging;

// ReSharper disable once CheckNamespace
namespace System;

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
            var teardown = new ExecutorTeardown<TResult>(executor, observer);
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
                            observer.OnNext(await selector(x));
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
