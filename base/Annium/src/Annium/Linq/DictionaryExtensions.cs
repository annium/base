using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Annium.Linq;

/// <summary>
/// Provides extension methods for working with dictionaries.
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    /// Removes all entries from the dictionary where the key matches the specified predicate.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="src">The source dictionary.</param>
    /// <param name="predicate">A function to test each key for a condition.</param>
    public static void RemoveAll<TKey, TValue>(this IDictionary<TKey, TValue> src, Func<TKey, bool> predicate)
        where TKey : notnull
    {
        foreach (var key in src.Keys.Where(predicate).ToArray())
            src.Remove(key);
    }

    /// <summary>
    /// Removes all entries from the dictionary where the value matches the specified predicate.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="src">The source dictionary.</param>
    /// <param name="predicate">A function to test each value for a condition.</param>
    public static void RemoveAll<TKey, TValue>(this IDictionary<TKey, TValue> src, Func<TValue, bool> predicate)
    {
        foreach (var item in src.Where(x => predicate(x.Value)).ToArray())
            src.Remove(item);
    }

    /// <summary>
    /// Removes all entries from the dictionary where the key-value pair matches the specified predicate.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="src">The source dictionary.</param>
    /// <param name="predicate">A function to test each key-value pair for a condition.</param>
    public static void RemoveAll<TKey, TValue>(this IDictionary<TKey, TValue> src, Func<TKey, TValue, bool> predicate)
    {
        foreach (var item in src.Where(x => predicate(x.Key, x.Value)).ToArray())
            src.Remove(item);
    }

    /// <summary>
    /// Merges collection of key-value pairs into a given dictionary, with values from the target collection taking precedence.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the collections.</typeparam>
    /// <typeparam name="TValue">The type of the values in the collections.</typeparam>
    /// <param name="source">The source collection of key-value pairs.</param>
    /// <param name="target">The target collection of key-value pairs whose values will override source values for matching keys.</param>
    /// <param name="mergeBehavior">Merge behavior</param>
    /// <returns>A new dictionary containing the merged key-value pairs.</returns>
    public static Dictionary<TKey, TValue> Merge<TKey, TValue>(
        this Dictionary<TKey, TValue> source,
        IEnumerable<KeyValuePair<TKey, TValue>> target,
        MergeBehavior mergeBehavior
    )
        where TKey : notnull
    {
        if (mergeBehavior is MergeBehavior.KeepTarget)
            foreach (var (key, value) in target)
                source[key] = value;
        else
            foreach (var (key, value) in target)
                source.TryAdd(key, value);

        return source;
    }

    /// <summary>
    /// Merges two collections of key-value pairs into a single dictionary, with values from the target collection taking precedence.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the collections.</typeparam>
    /// <typeparam name="TValue">The type of the values in the collections.</typeparam>
    /// <param name="source">The source collection of key-value pairs.</param>
    /// <param name="target">The target collection of key-value pairs whose values will override source values for matching keys.</param>
    /// <param name="getKey">Key resolver</param>
    /// <param name="mergeBehavior">Merge behavior</param>
    /// <returns>A new dictionary containing the merged key-value pairs.</returns>
    public static Dictionary<TKey, TValue> Merge<TKey, TValue>(
        this Dictionary<TKey, TValue> source,
        IEnumerable<TValue> target,
        Func<TValue, TKey> getKey,
        MergeBehavior mergeBehavior
    )
        where TKey : notnull
    {
        if (mergeBehavior is MergeBehavior.KeepTarget)
            foreach (var value in target)
                source[getKey(value)] = value;
        else
            foreach (var value in target)
                source.TryAdd(getKey(value), value);

        return source;
    }

    /// <summary>
    /// Gets the value associated with the specified key, throwing an exception if the key is not found.
    /// </summary>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="dictionary">The source dictionary.</param>
    /// <param name="key">The key to locate.</param>
    /// <returns>The value associated with the specified key.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the key is null or not found in the dictionary.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TValue MapValue<TValue>(this IReadOnlyDictionary<string, TValue> dictionary, string? key)
    {
        return key is not null && dictionary.TryGetValue(key, out var value)
            ? value
            : throw new ArgumentOutOfRangeException(nameof(key));
    }

    /// <summary>
    /// Attempts to get the value associated with the specified key, returning the default value if the key is not found.
    /// </summary>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="dictionary">The source dictionary.</param>
    /// <param name="key">The key to locate.</param>
    /// <returns>The value associated with the specified key, or the default value if the key is not found.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TValue? TryMapValue<TValue>(this IReadOnlyDictionary<string, TValue> dictionary, string? key)
    {
        return key is not null && dictionary.TryGetValue(key, out var value) ? value : default;
    }
}
