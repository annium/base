using System.Collections.Concurrent;
using System.Collections.Generic;
using NodaTime;

namespace Annium.Collections.Generic;

public class ExpiringDictionary<TKey, TValue>
    where TKey : notnull
{
    private readonly ITimeProvider _timeProvider;
    private readonly ConcurrentDictionary<TKey, Item> _data = new();

    public ExpiringDictionary(ITimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public void Add(TKey key, TValue value, Duration ttl)
    {
        var item = new Item(value, _timeProvider.Now + ttl);
        _data.AddOrUpdate(key, item, (_, _) => item);
    }

    public bool TryGet(TKey key, out TValue value)
    {
        Cleanup();

        var result = _data.TryGetValue(key, out var item);
        value = item is null ? default! : item.Value;

        return result;
    }

    public TValue Get(TKey key)
    {
        Cleanup();

        if (!_data.TryGetValue(key, out var item))
            throw new KeyNotFoundException($"Key {key} is missing in collection");

        return item.Value;
    }

    public bool ContainsKey(TKey key)
    {
        Cleanup();

        return _data.ContainsKey(key);
    }

    public bool Remove(TKey key, out TValue value)
    {
        Cleanup();

        var result = _data.TryRemove(key, out var item);
        value = item is null ? default! : item.Value;

        return result;
    }

    public void Clear()
    {
        _data.Clear();
    }

    private void Cleanup()
    {
        var now = _timeProvider.Now;
        var pairs = _data.ToArray();
        foreach (var (key, value) in pairs)
        {
            if (value.Expires < now)
                _data.TryRemove(key, out _);
        }
    }

    private record Item(TValue Value, Instant Expires);
}