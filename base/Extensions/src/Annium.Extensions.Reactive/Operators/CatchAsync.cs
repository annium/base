using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Annium.Extensions.Reactive.Operators;

/// <summary>
/// Provides asynchronous error handling operators for observables
/// </summary>
public static class CatchAsyncOperatorExtensions
{
    /// <summary>
    /// Catches exceptions of a specific type and handles them asynchronously by returning a replacement observable
    /// </summary>
    /// <typeparam name="TSource">The type of items emitted by the source observable</typeparam>
    /// <typeparam name="TException">The type of exception to catch</typeparam>
    /// <param name="source">The source observable to monitor for exceptions</param>
    /// <param name="handler">Asynchronous function that handles the exception and returns a replacement observable</param>
    /// <returns>An observable that continues with the replacement observable when the specified exception occurs</returns>
    public static IObservable<TSource> CatchAsync<TSource, TException>(
        this IObservable<TSource> source,
        Func<TException, Task<IObservable<TSource>>> handler
    )
        where TException : Exception =>
        source.Catch<TSource, TException>(e => Observable.FromAsync(() => handler(e)).SelectMany(x => x));
}
