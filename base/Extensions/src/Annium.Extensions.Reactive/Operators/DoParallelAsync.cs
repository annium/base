using System.Reactive.Linq;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System;

public static class DoParallelAsyncOperatorExtensions
{
    public static IObservable<TSource> DoParallelAsync<TSource>(
        this IObservable<TSource> source,
        Func<TSource, Task> handle
    )
        => source.SelectMany(x => Observable.FromAsync(async () =>
        {
            await handle(x);
            return x;
        }));

    public static IObservable<TSource> DoParallelAsync<TSource>(
        this IObservable<TSource> source,
        Func<TSource, int, Task> selector
    )
        => source.SelectMany((x, i) => Observable.FromAsync(async () =>
        {
            await selector(x, i);
            return x;
        }));
}