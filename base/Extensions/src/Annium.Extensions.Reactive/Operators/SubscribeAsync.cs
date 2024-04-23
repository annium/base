using System.Threading.Tasks;
using Annium;
using Annium.Execution.Background;
using Annium.Logging;

// ReSharper disable once CheckNamespace
namespace System;

public static class SubscribeAsyncOperatorExtensions
{
    public static IAsyncDisposable SubscribeAsync<T>(this IObservable<T> source, Func<Exception, ValueTask> onError)
    {
        var executor = Executor.Parallel<IObservable<T>>(VoidLogger.Instance).Start();
        var subscription = source.Subscribe(Noop, ex => executor.Schedule(() => onError(ex)));

        return Disposable.Create(async () =>
        {
            subscription.Dispose();
            await executor.DisposeAsync();
        });
    }

    public static IAsyncDisposable SubscribeAsync<T>(this IObservable<T> source, Func<ValueTask> onCompleted)
    {
        var executor = Executor.Parallel<IObservable<T>>(VoidLogger.Instance).Start();
        var subscription = source.Subscribe(Noop, () => executor.Schedule(onCompleted));

        return Disposable.Create(async () =>
        {
            subscription.Dispose();
            await executor.DisposeAsync();
        });
    }

    public static IAsyncDisposable SubscribeAsync<T>(
        this IObservable<T> source,
        Func<Exception, ValueTask> onError,
        Func<ValueTask> onCompleted
    )
    {
        var executor = Executor.Parallel<IObservable<T>>(VoidLogger.Instance).Start();
        var subscription = source.Subscribe(
            Noop,
            ex => executor.Schedule(() => onError(ex)),
            () => executor.Schedule(onCompleted)
        );

        return Disposable.Create(async () =>
        {
            subscription.Dispose();
            await executor.DisposeAsync();
        });
    }

    public static IAsyncDisposable SubscribeAsync<T>(this IObservable<T> source, Func<T, ValueTask> onNext)
    {
        var executor = Executor.Parallel<IObservable<T>>(VoidLogger.Instance).Start();
        var subscription = source.Subscribe(x => executor.Schedule(() => onNext(x)));

        return Disposable.Create(async () =>
        {
            subscription.Dispose();
            await executor.DisposeAsync();
        });
    }

    public static IAsyncDisposable SubscribeAsync<T>(
        this IObservable<T> source,
        Func<T, ValueTask> onNext,
        Func<Exception, ValueTask> onError
    )
    {
        var executor = Executor.Parallel<IObservable<T>>(VoidLogger.Instance).Start();
        var subscription = source.Subscribe(
            x => executor.Schedule(() => onNext(x)),
            ex => executor.Schedule(() => onError(ex))
        );

        return Disposable.Create(async () =>
        {
            subscription.Dispose();
            await executor.DisposeAsync();
        });
    }

    public static IAsyncDisposable SubscribeAsync<T>(
        this IObservable<T> source,
        Func<T, ValueTask> onNext,
        Func<ValueTask> onCompleted
    )
    {
        var executor = Executor.Parallel<IObservable<T>>(VoidLogger.Instance).Start();
        var subscription = source.Subscribe(
            x => executor.Schedule(() => onNext(x)),
            () => executor.Schedule(onCompleted)
        );

        return Disposable.Create(async () =>
        {
            subscription.Dispose();
            await executor.DisposeAsync();
        });
    }

    public static IAsyncDisposable SubscribeAsync<T>(
        this IObservable<T> source,
        Func<T, ValueTask> onNext,
        Func<Exception, ValueTask> onError,
        Func<ValueTask> onCompleted
    )
    {
        var executor = Executor.Parallel<IObservable<T>>(VoidLogger.Instance).Start();
        var subscription = source.Subscribe(
            x => executor.Schedule(() => onNext(x)),
            ex => executor.Schedule(() => onError(ex)),
            () => executor.Schedule(onCompleted)
        );

        return Disposable.Create(async () =>
        {
            subscription.Dispose();
            await executor.DisposeAsync();
        });
    }

    private static void Noop<T>(T _) { }
}
