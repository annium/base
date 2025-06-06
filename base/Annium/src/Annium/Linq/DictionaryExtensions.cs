using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Annium.Linq;

public static class DictionaryExtensions
{
    public static void RemoveAll<TKey, TValue>(this IDictionary<TKey, TValue> src, Func<TKey, bool> predicate)
        where TKey : notnull
    {
        foreach (var key in src.Keys.Where(predicate).ToArray())
            src.Remove(key);
    }

    public static void RemoveAll<TKey, TValue>(this IDictionary<TKey, TValue> src, Func<TValue, bool> predicate)
    {
        foreach (var item in src.Where(x => predicate(x.Value)).ToArray())
            src.Remove(item);
    }

    public static void RemoveAll<TKey, TValue>(this IDictionary<TKey, TValue> src, Func<TKey, TValue, bool> predicate)
    {
        foreach (var item in src.Where(x => predicate(x.Key, x.Value)).ToArray())
            src.Remove(item);
    }

    public static Dictionary<TKey, TValue> Merge<TKey, TValue>(
        this IEnumerable<KeyValuePair<TKey, TValue>> source,
        IEnumerable<KeyValuePair<TKey, TValue>> target
    )
        where TKey : notnull
    {
        var values = source.ToDictionary(x => x.Key, x => x.Value);
        foreach (var (key, value) in target)
            values[key] = value;

        return values;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TValue MapValue<TValue>(this IReadOnlyDictionary<string, TValue> dictionary, string? key)
    {
        return key is not null && dictionary.TryGetValue(key, out var value)
            ? value
            : throw new ArgumentOutOfRangeException(nameof(key));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TValue? TryMapValue<TValue>(this IReadOnlyDictionary<string, TValue> dictionary, string? key)
    {
        return key is not null && dictionary.TryGetValue(key, out var value) ? value : default;
    }
}
