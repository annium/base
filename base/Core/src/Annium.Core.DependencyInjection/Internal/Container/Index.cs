using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.DependencyInjection.Internal.Builders.Registrations;

namespace Annium.Core.DependencyInjection.Internal.Container;

internal class Index<TKey, TValue> : IIndex<TKey, TValue>
    where TKey : notnull
    where TValue : notnull
{
    public int Count => _items.Count;
    public IEnumerable<TKey> Keys => _items.Keys;
    public IEnumerable<TValue> Values => _items.Values.Select(x => x.Value);

    public TValue this[TKey key] => _items.TryGetValue(key, out var item)
        ? item.Value
        : throw new InvalidOperationException(
            $"No {typeof(TValue).FriendlyName()} value registered for key {typeof(TKey).FriendlyName()} {key}"
        );

    private readonly IReadOnlyDictionary<TKey, Lazy<TValue>> _items;

    public Index(IEnumerable<KeyValue<TKey, TValue>> source)
    {
        var items = new Dictionary<TKey, Lazy<TValue>>();

        foreach (var item in source)
        {
            if (!items.TryAdd(item.Key, item.Value))
                throw new InvalidOperationException(
                    $"Ambiguous match of {typeof(TValue).FriendlyName()} values registered for key {typeof(TKey).FriendlyName()} {item.Key}"
                );
        }

        _items = items;
    }

    public bool ContainsKey(TKey key) =>
        TryGetValue(key, out _);

    public bool TryGetValue(TKey key, out TValue value)
    {
        if (_items.TryGetValue(key, out var item))
        {
            value = item.Value;
            return true;
        }

        value = default!;
        return false;
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _items
        .Select(x => new KeyValuePair<TKey, TValue>(x.Key, x.Value.Value))
        .GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
}