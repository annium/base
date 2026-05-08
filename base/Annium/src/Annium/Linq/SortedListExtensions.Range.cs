using System;
using System.Collections.Generic;
using Annium.Collections.Generic;

namespace Annium.Linq;

/// <summary>Provides extension methods for working with ranges in sorted lists.</summary>
public static class SortedListRangeExtensions
{
    /// <summary>Adds a range of key-value pairs to the sorted list, throwing an exception if any key already exists.</summary>
    /// <typeparam name="TKey">The type of the keys in the sorted list.</typeparam>
    /// <typeparam name="TValue">The type of the values in the sorted list.</typeparam>
    /// <param name="source">The sorted list to add the range to.</param>
    /// <param name="range">The range of key-value pairs to add.</param>
    /// <exception cref="InvalidOperationException">Thrown when a key from <paramref name="range"/> already exists in <paramref name="source"/>.</exception>
    public static void AddRange<TKey, TValue>(
        this SortedList<TKey, TValue> source,
        IReadOnlyDictionary<TKey, TValue> range
    )
        where TKey : notnull
    {
        foreach (var (key, value) in range)
        {
            if (source.ContainsKey(key))
                throw new InvalidOperationException($"Trying to add duplicate key {key}");
            source.Add(key, value);
        }
    }

    /// <summary>Sets a range of key-value pairs in the sorted list, replacing any existing values for duplicate keys.</summary>
    /// <typeparam name="TKey">The type of the keys in the sorted list.</typeparam>
    /// <typeparam name="TValue">The type of the values in the sorted list.</typeparam>
    /// <param name="source">The sorted list to set the range in.</param>
    /// <param name="range">The range of key-value pairs to set.</param>
    public static void SetRange<TKey, TValue>(
        this SortedList<TKey, TValue> source,
        IReadOnlyDictionary<TKey, TValue> range
    )
        where TKey : notnull
    {
        foreach (var (key, value) in range)
            source[key] = value;
    }

    /// <summary>Gets a span of the sorted list between the specified start and end keys.</summary>
    /// <typeparam name="TKey">The type of the keys in the sorted list.</typeparam>
    /// <typeparam name="TValue">The type of the values in the sorted list.</typeparam>
    /// <param name="source">The sorted list to get the range from.</param>
    /// <param name="start">The start key of the range.</param>
    /// <param name="end">The end key of the range.</param>
    /// <returns>A span of the sorted list between the specified keys, or null if the keys are not found.</returns>
    public static ISortedListSpan<TKey, TValue>? GetRange<TKey, TValue>(
        this SortedList<TKey, TValue> source,
        TKey start,
        TKey end
    )
        where TKey : notnull
    {
        var startIndex = source.IndexOfKey(start);
        var endIndex = source.IndexOfKey(end);

        if (startIndex < 0 || endIndex < 0)
            return null;

        return new SortedListSpan<TKey, TValue>(source, startIndex, endIndex - startIndex + 1);
    }
}
