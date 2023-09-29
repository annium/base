using System.Reactive.Linq;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System;

public static class DoSequentialAsyncOperatorExtensions
{
    public static IObservable<TSource> DoSequentialAsync<TSource>(
        this IObservable<TSource> source,
        Func<TSource, Task> handle
    )
        => source.Select(x => Observable.FromAsync(async () =>
        {
            await handle(x);
            return x;
        })).Concat();

    public static IObservable<TSource> DoSequentialAsync<TSource>(
        this IObservable<TSource> source,
        Func<TSource, int, Task> handle
    )
        => source.Select((x, i) => Observable.FromAsync(async () =>
        {
            await handle(x, i);
            return x;
        })).Concat();
}