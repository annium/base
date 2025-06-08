using System.Threading.Tasks;
using Annium;
using Annium.Execution.Background;
using Annium.Logging;

// ReSharper disable once CheckNamespace
namespace System;

/// <summary>
/// Provides asynchronous subscription operators for observables
/// </summary>
public static class SubscribeAsyncOperatorExtensions
{
    /// <summary>
    /// Subscribes to an observable with an asynchronous error handler
    /// </summary>
    /// <typeparam name="T">The type of items emitted by the observable</typeparam>
    /// <param name="source">The source observable to subscribe to</param>
    /// <param name="onError">Asynchronous function to handle errors</param>
    /// <returns>An async disposable that can be used to unsubscribe</returns>
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

    /// <summary>
    /// Subscribes to an observable with an asynchronous completion handler
    /// </summary>
    /// <typeparam name="T">The type of items emitted by the observable</typeparam>
    /// <param name="source">The source observable to subscribe to</param>
    /// <param name="onCompleted">Asynchronous function to handle completion</param>
    /// <returns>An async disposable that can be used to unsubscribe</returns>
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

    /// <summary>
    /// Subscribes to an observable with asynchronous error and completion handlers
    /// </summary>
    /// <typeparam name="T">The type of items emitted by the observable</typeparam>
    /// <param name="source">The source observable to subscribe to</param>
    /// <param name="onError">Asynchronous function to handle errors</param>
    /// <param name="onCompleted">Asynchronous function to handle completion</param>
    /// <returns>An async disposable that can be used to unsubscribe</returns>
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

    /// <summary>
    /// Subscribes to an observable with an asynchronous value handler
    /// </summary>
    /// <typeparam name="T">The type of items emitted by the observable</typeparam>
    /// <param name="source">The source observable to subscribe to</param>
    /// <param name="onNext">Asynchronous function to handle each emitted value</param>
    /// <returns>An async disposable that can be used to unsubscribe</returns>
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

    /// <summary>
    /// Subscribes to an observable with asynchronous value and error handlers
    /// </summary>
    /// <typeparam name="T">The type of items emitted by the observable</typeparam>
    /// <param name="source">The source observable to subscribe to</param>
    /// <param name="onNext">Asynchronous function to handle each emitted value</param>
    /// <param name="onError">Asynchronous function to handle errors</param>
    /// <returns>An async disposable that can be used to unsubscribe</returns>
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

    /// <summary>
    /// Subscribes to an observable with asynchronous value and completion handlers
    /// </summary>
    /// <typeparam name="T">The type of items emitted by the observable</typeparam>
    /// <param name="source">The source observable to subscribe to</param>
    /// <param name="onNext">Asynchronous function to handle each emitted value</param>
    /// <param name="onCompleted">Asynchronous function to handle completion</param>
    /// <returns>An async disposable that can be used to unsubscribe</returns>
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

    /// <summary>
    /// Subscribes to an observable with asynchronous value, error, and completion handlers
    /// </summary>
    /// <typeparam name="T">The type of items emitted by the observable</typeparam>
    /// <param name="source">The source observable to subscribe to</param>
    /// <param name="onNext">Asynchronous function to handle each emitted value</param>
    /// <param name="onError">Asynchronous function to handle errors</param>
    /// <param name="onCompleted">Asynchronous function to handle completion</param>
    /// <returns>An async disposable that can be used to unsubscribe</returns>
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

    /// <summary>
    /// No-operation method for ignoring values
    /// </summary>
    /// <typeparam name="T">The type of the value to ignore</typeparam>
    /// <param name="_">The value to ignore</param>
    private static void Noop<T>(T _) { }
}
