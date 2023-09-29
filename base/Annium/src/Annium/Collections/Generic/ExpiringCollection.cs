using System.Collections.Concurrent;
using NodaTime;

namespace Annium.Collections.Generic;

public class ExpiringCollection<T>
    where T : notnull
{
    private readonly ITimeProvider _timeProvider;
    private readonly ConcurrentDictionary<T, Instant> _data = new();

    public ExpiringCollection(ITimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public void Add(T item, Duration ttl)
    {
        var expires = _timeProvider.Now + ttl;
        _data.AddOrUpdate(item, expires, (_, _) => expires);
    }

    public bool Contains(T item)
    {
        Cleanup();

        return _data.ContainsKey(item);
    }

    public bool Remove(T item)
    {
        Cleanup();

        return _data.TryRemove(item, out _);
    }

    public void Clear()
    {
        _data.Clear();
    }

    private void Cleanup()
    {
        var now = _timeProvider.Now;
        var pairs = _data.ToArray();
        foreach (var (item, expires) in pairs)
        {
            if (expires < now)
                _data.TryRemove(item, out _);
        }
    }
}