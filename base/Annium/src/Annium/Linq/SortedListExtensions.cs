using System.Collections.Generic;
using Annium.Collections.Generic;

namespace Annium.Linq;

/// <summary>Provides extension methods for working with sorted lists.</summary>
public static class SortedListExtensions
{
    /// <summary>Creates a span view over a portion of a sorted list.</summary>
    /// <typeparam name="TKey">The type of keys in the sorted list.</typeparam>
    /// <typeparam name="TValue">The type of values in the sorted list.</typeparam>
    /// <param name="items">The source sorted list.</param>
    /// <param name="start">The starting index of the span.</param>
    /// <param name="count">The number of elements to include in the span.</param>
    /// <returns>A sorted list span representing the specified portion of the source list.</returns>
    public static ISortedListSpan<TKey, TValue> ToListSpan<TKey, TValue>(
        this SortedList<TKey, TValue> items,
        int start,
        int count
    )
        where TKey : notnull => new SortedListSpan<TKey, TValue>(items, start, count);
}
