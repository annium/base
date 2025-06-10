using System.Collections.Generic;
using Annium.Collections.Generic;

namespace Annium.Linq;

/// <summary>Provides extension methods for working with read-only lists.</summary>
public static class ReadOnlyListExtensions
{
    /// <summary>Creates a span view over a portion of a read-only list.</summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="items">The source read-only list.</param>
    /// <param name="start">The starting index of the span.</param>
    /// <param name="count">The number of elements to include in the span.</param>
    /// <returns>A list span representing the specified portion of the source list.</returns>
    public static IListSpan<T> ToListSpan<T>(this IReadOnlyList<T> items, int start, int count) =>
        new ListSpan<T>(items, start, count);
}
