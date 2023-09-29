using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System;

public static class SelectSequentialAsyncOperatorExtensions
{
    public static IObservable<TResult> SelectSequentialAsync<TSource, TResult>(
        this IObservable<TSource> source,
        Func<TSource, Task<TResult>> selector
    )
        => source.Select(x => Observable.FromAsync(() => selector(x))).Concat();

    public static IObservable<TResult> SelectSequentialAsync<TSource, TResult>(
        this IObservable<TSource> source,
        Func<TSource, int, Task<TResult>> selector
    )
        => source.Select((x, i) => Observable.FromAsync(() => selector(x, i))).Concat();

    public static IObservable<Unit> SelectSequentialAsync<TSource>(
        this IObservable<TSource> source,
        Func<TSource, Task> selector
    )
        => source.Select(x => Observable.FromAsync(() => selector(x))).Concat();

    public static IObservable<Unit> SelectSequentialAsync<TSource>(
        this IObservable<TSource> source,
        Func<TSource, int, Task> selector
    )
        => source.Select((x, i) => Observable.FromAsync(() => selector(x, i))).Concat();
}