using System.Collections.Generic;

namespace Annium.Collections.Generic;

/// <summary>
/// Defines a span of key-value pairs from a sorted list with the ability to move the span's position.
/// </summary>
/// <typeparam name="TKey">The type of the keys in the sorted list.</typeparam>
/// <typeparam name="TValue">The type of the values in the sorted list.</typeparam>
public interface ISortedListSpan<TKey, TValue> : IReadOnlyIndexedSpan<KeyValuePair<TKey, TValue>>
    where TKey : notnull
{
    /// <summary>
    /// Moves the span by the specified offset.
    /// </summary>
    /// <param name="offset">The number of positions to move the span.</param>
    /// <returns>True if the move was successful; otherwise, false.</returns>
    bool Move(int offset);
}
