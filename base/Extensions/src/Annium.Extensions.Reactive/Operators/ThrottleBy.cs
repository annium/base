using System.Collections.Concurrent;
using System.Reactive.Linq;
using Annium;
using NodaTime;

// ReSharper disable once CheckNamespace
namespace System;

public static class ThrottleByOperatorExtensions
{
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

            return source.Subscribe(x =>
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
            }, observer.OnError, observer.OnCompleted);
        });
    }
}