using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System;

public static class SelectParallelAsyncOperatorExtensions
{
    public static IObservable<TResult> SelectParallelAsync<TSource, TResult>(
        this IObservable<TSource> source,
        Func<TSource, Task<TResult>> selector
    )
        => source.SelectMany(x => Observable.FromAsync(() => selector(x)));

    public static IObservable<TResult> SelectParallelAsync<TSource, TResult>(
        this IObservable<TSource> source,
        Func<TSource, int, Task<TResult>> selector
    )
        => source.SelectMany((x, i) => Observable.FromAsync(() => selector(x, i)));

    public static IObservable<Unit> SelectParallelAsync<TSource>(
        this IObservable<TSource> source,
        Func<TSource, Task> selector
    )
        => source.SelectMany(x => Observable.FromAsync(() => selector(x)));

    public static IObservable<Unit> SelectParallelAsync<TSource>(
        this IObservable<TSource> source,
        Func<TSource, int, Task> selector
    )
        => source.SelectMany((x, i) => Observable.FromAsync(() => selector(x, i)));
}