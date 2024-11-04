using System.Threading.Tasks;
using Annium;
using Annium.Execution.Background;
using Annium.Logging;

// ReSharper disable once CheckNamespace
namespace System;

public static class SubscribeAsyncOperatorExtensions
{
#pragma warning disable VSTHRD200
    public static IAsyncDisposable SubscribeAsync<T>(this IObservable<T> source, Func<Exception, ValueTask> onError)
#pragma warning restore VSTHRD200
    {
        var executor = Executor.Parallel<IObservable<T>>(VoidLogger.Instance).Start();
        var subscription = source.Subscribe(Noop, ex => executor.Schedule(() => onError(ex)));

        return Disposable.Create(async () =>
        {
            await subscription.DisposeAsync();
            await executor.DisposeAsync();
        });
    }

#pragma warning disable VSTHRD200
    public static IAsyncDisposable SubscribeAsync<T>(this IObservable<T> source, Func<ValueTask> onCompleted)
#pragma warning restore VSTHRD200
    {
        var executor = Executor.Parallel<IObservable<T>>(VoidLogger.Instance).Start();
        var subscription = source.Subscribe(Noop, () => executor.Schedule(onCompleted));

        return Disposable.Create(async () =>
        {
            await subscription.DisposeAsync();
            await executor.DisposeAsync();
        });
    }

#pragma warning disable VSTHRD200
    public static IAsyncDisposable SubscribeAsync<T>(
#pragma warning restore VSTHRD200
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
            await subscription.DisposeAsync();
            await executor.DisposeAsync();
        });
    }

#pragma warning disable VSTHRD200
    public static IAsyncDisposable SubscribeAsync<T>(this IObservable<T> source, Func<T, ValueTask> onNext)
#pragma warning restore VSTHRD200
    {
        var executor = Executor.Parallel<IObservable<T>>(VoidLogger.Instance).Start();
        var subscription = source.Subscribe(x => executor.Schedule(() => onNext(x)));

        return Disposable.Create(async () =>
        {
            await subscription.DisposeAsync();
            await executor.DisposeAsync();
        });
    }

#pragma warning disable VSTHRD200
    public static IAsyncDisposable SubscribeAsync<T>(
#pragma warning restore VSTHRD200
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
            await subscription.DisposeAsync();
            await executor.DisposeAsync();
        });
    }

#pragma warning disable VSTHRD200
    public static IAsyncDisposable SubscribeAsync<T>(
#pragma warning restore VSTHRD200
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
            await subscription.DisposeAsync();
            await executor.DisposeAsync();
        });
    }

#pragma warning disable VSTHRD200
    public static IAsyncDisposable SubscribeAsync<T>(
#pragma warning restore VSTHRD200
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
            await subscription.DisposeAsync();
            await executor.DisposeAsync();
        });
    }

    private static void Noop<T>(T _) { }
}
