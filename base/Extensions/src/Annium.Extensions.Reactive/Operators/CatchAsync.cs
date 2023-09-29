using System.Reactive.Linq;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System;

public static class CatchAsyncOperatorExtensions
{
    public static IObservable<TSource> CatchAsync<TSource, TException>(
        this IObservable<TSource> source,
        Func<TException, Task<IObservable<TSource>>> handler
    )
        where TException : Exception
        => source.Catch<TSource, TException>(
            e => Observable
                .FromAsync(() => handler(e))
                .SelectMany(x => x)
        );
}