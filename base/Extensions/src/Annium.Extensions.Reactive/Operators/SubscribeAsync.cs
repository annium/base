using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Annium.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System;

public static class SubscribeAsyncOperatorExtensions
{
    public static void SubscribeAsync<T>(
        this IObservable<T> source,
        Func<Exception, Task> onError,
        Func<Task> onCompleted,
        CancellationToken ct
    )
        => source.Subscribe(Noop, Wrap(onError), Wrap(onCompleted), ct);

    public static void SubscribeAsync<T>(
        this IObservable<T> source,
        Func<Exception, Task> onError,
        CancellationToken ct
    )
        => source.Subscribe(Noop, Wrap(onError), Noop, ct);

    public static void SubscribeAsync<T>(
        this IObservable<T> source,
        Func<Task> onCompleted,
        CancellationToken ct
    )
        => source.Subscribe(Noop, Throw, Wrap(onCompleted), ct);

    public static IDisposable SubscribeAsync<T>(
        this IObservable<T> source,
        Func<Exception, Task> onError,
        Func<Task> onCompleted
    )
        => source.Subscribe(Noop, Wrap(onError), Wrap(onCompleted));

    public static IDisposable SubscribeAsync<T>(
        this IObservable<T> source,
        Func<Exception, Task> onError
    )
        => source.Subscribe(Noop, Wrap(onError), Noop);

    public static IDisposable SubscribeAsync<T>(
        this IObservable<T> source,
        Func<Task> onCompleted
    )
        => source.Subscribe(Noop, Throw, Wrap(onCompleted));

    private static Action Wrap(Func<Task> handle) => () => handle().Await();
    private static Action<T> Wrap<T>(Func<T, Task> handle) => x => handle(x).Await();

    private static void Noop<T>(T x)
    {
    }

    private static void Noop()
    {
    }

    private static void Throw(Exception ex) => ExceptionDispatchInfo.Capture(ex).Throw();
}