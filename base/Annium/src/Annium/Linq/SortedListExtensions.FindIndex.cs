using System;
using System.Collections.Generic;

namespace Annium.Linq;

/// <summary>Provides extension methods for finding indices in sorted lists.</summary>
public static class SortedListFindIndexExtensions
{
    /// <summary>Searches for the specified key and returns the zero-based index of the first occurrence within the entire sorted list.</summary>
    /// <typeparam name="TKey">The type of keys in the sorted list.</typeparam>
    /// <typeparam name="TValue">The type of values in the sorted list.</typeparam>
    /// <param name="source">The source sorted list.</param>
    /// <param name="key">The key to locate in the sorted list.</param>
    /// <returns>The zero-based index of the first occurrence of the key within the entire sorted list, if found; otherwise, -1.</returns>
    public static int FindIndex<TKey, TValue>(this SortedList<TKey, TValue> source, TKey key)
        where TKey : notnull
    {
        var i = 0;

        foreach (var pair in source)
            if (pair.Key.Equals(key))
                return i;
            else
                i++;

        return -1;
    }

    /// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the entire sorted list.</summary>
    /// <typeparam name="TKey">The type of keys in the sorted list.</typeparam>
    /// <typeparam name="TValue">The type of values in the sorted list.</typeparam>
    /// <param name="source">The source sorted list.</param>
    /// <param name="match">The predicate that defines the conditions of the element to search for.</param>
    /// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by match, if found; otherwise, -1.</returns>
    public static int FindIndex<TKey, TValue>(this SortedList<TKey, TValue> source, Predicate<TValue> match)
        where TKey : notnull
    {
        var i = 0;

        foreach (var pair in source)
            if (match(pair.Value))
                return i;
            else
                i++;

        return -1;
    }
}
