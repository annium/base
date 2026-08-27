using System.Collections.Concurrent;
using System.Reactive.Linq;
using Annium;
using NodaTime;

// ReSharper disable once CheckNamespace
namespace System;

/// <summary>
/// Provides throttling operators that group values by key
/// </summary>
public static class ThrottleByOperatorExtensions
{
    /// <summary>
    /// Throttles values emitted by the source observable by a key-based interval, allowing only the first occurrence of each key within the specified time window
    /// </summary>
    /// <typeparam name="TSource">The type of items emitted by the source observable</typeparam>
    /// <typeparam name="TKey">The type of the key used for throttling</typeparam>
    /// <param name="source">The source observable to throttle</param>
    /// <param name="getKey">Function to extract the throttling key from each value</param>
    /// <param name="interval">The time interval for throttling each key</param>
    /// <returns>An observable that emits values throttled by key and interval</returns>
    public static IObservable<TSource> ThrottleBy<TSource, TKey>(
        this IObservable<TSource> source,
        Func<TSource, TKey> getKey,
        Duration interval
    )
        where TKey : notnull
    {
        var clock = SystemClock.Instance;
        var intervalMs = interval.TotalMilliseconds.FloorInt64();

        return Observable.Create<TSource>(observer =>
        {
            var keys = new ConcurrentDictionary<TKey, long>();

            return source.Subscribe(
                x =>
                {
                    var now = clock.GetCurrentInstant().ToUnixTimeMilliseconds();
                    var key = getKey(x);

                    // the stored stamp is the moment this key last emitted. Deciding by comparing a
                    // recomputed stamp to the stored one could not tell "we just advanced it" from "it
                    // already held that value", so a burst arriving within a single millisecond passed
                    // through whole — exactly the case throttling exists for. Emission is now claimed:
                    // only the caller that wins the add or the update gets to emit.
                    var emit = false;
                    while (true)
                    {
                        if (!keys.TryGetValue(key, out var last))
                        {
                            if (!keys.TryAdd(key, now))
                                continue;

                            emit = true;
                            break;
                        }

                        if (now - last < intervalMs)
                            break;

                        if (!keys.TryUpdate(key, now, last))
                            continue;

                        emit = true;
                        break;
                    }

                    if (emit)
                        observer.OnNext(x);
                },
                observer.OnError,
                observer.OnCompleted
            );
        });
    }
}
