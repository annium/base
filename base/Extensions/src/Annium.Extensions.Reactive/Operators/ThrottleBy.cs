using System;
using System.Collections.Concurrent;
using System.Reactive.Linq;
using NodaTime;

namespace Annium.Extensions.Reactive.Operators;

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

                    var stamp = keys.AddOrUpdate(
                        key,
                        static (_, data) => data.now + data.interval,
                        static (_, value, data) => value <= data.now ? data.now + data.interval : value,
                        (now, interval: intervalMs)
                    );

                    if (stamp == now + intervalMs)
                        observer.OnNext(x);
                },
                observer.OnError,
                observer.OnCompleted
            );
        });
    }
}
