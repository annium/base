using System.Reactive.Linq;
using System.Threading.Tasks;
using Annium.Execution.Background;
using Annium.Logging;

// ReSharper disable once CheckNamespace
namespace System;

public static class DoParallelAsyncOperatorExtensions
{
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
                () => executor.DisposeAsync().AsTask().ContinueWith(_ => observer.OnCompleted())
            );
        });
    }
}
