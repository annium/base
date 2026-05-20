using System;
using System.Collections;
using System.Collections.Generic;
using Annium.Internal.Collections.Generic;

namespace Annium.Collections.Generic;

/// <summary>
/// Represents a span of key-value pairs from a sorted list, with the ability to move the span's position.
/// </summary>
/// <remarks>
/// Declared as <c>record</c> so two spans with the same public state (<see cref="Count"/>, <see cref="Start"/>,
/// <see cref="End"/>) compare equal — relied on by <c>SortedListExtensions.GetChunks</c> tests and other
/// range-comparison call sites. <b>Caveat:</b> the compiler-synthesized <c>with</c>-expression can reach
/// <see cref="Start"/> directly (its setter is <c>private set</c>, but record copy ctors bypass that), so
/// callers can construct an out-of-range span via <c>span with { Start = -1 }</c>. The <see cref="Move(int)"/>
/// API is the only sanctioned way to reposition the span and includes bounds checking.
/// </remarks>
/// <typeparam name="TKey">The type of the keys in the sorted list.</typeparam>
/// <typeparam name="TValue">The type of the values in the sorted list.</typeparam>
public sealed record SortedListSpan<TKey, TValue> : ISortedListSpan<TKey, TValue>
    where TKey : notnull
{
    /// <summary>
    /// Gets the number of elements in the span.
    /// </summary>
    public int Count { get; }

    /// <summary>
    /// Gets or sets the start index of the span.
    /// </summary>
    public int Start { get; private set; }

    /// <summary>
    /// Gets the end index of the span.
    /// </summary>
    public int End => Start + Count - 1;

    /// <summary>
    /// The underlying sorted list that this span references.
    /// </summary>
    private readonly SortedList<TKey, TValue> _collection;

    /// <summary>
    /// Initializes a new instance of the <see cref="SortedListSpan{TKey, TValue}"/> class.
    /// </summary>
    /// <param name="collection">The source sorted list to create a span from.</param>
    /// <param name="start">The starting index of the span.</param>
    /// <param name="count">The number of elements in the span.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the span parameters are invalid.</exception>
    public SortedListSpan(SortedList<TKey, TValue> collection, int start, int count)
    {
        IndexOutOfRangeMessage.ValidateSpanRange(start, count, collection.Count);

        _collection = collection;
        Start = start;
        Count = count;
    }

    /// <summary>
    /// Gets the key-value pair at the specified index within the span.
    /// </summary>
    /// <param name="index">The zero-based index of the key-value pair to get.</param>
    /// <returns>The key-value pair at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the index is out of range.</exception>
    public KeyValuePair<TKey, TValue> this[int index]
    {
        get
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index), IndexOutOfRangeMessage.For(index, Count));

            // Use parallel positional access on Keys / Values rather than Keys + key-based lookup:
            // (a) O(1) instead of O(log n); (b) the key and value come from the same offset in the
            // underlying SortedList, so the pair stays consistent even if the dictionary is mutated
            // between the two reads (the alternative `_collection[key]` does a binary search on Keys
            // and would return a value paired with a different position's key).
            return KeyValuePair.Create(_collection.Keys[Start + index], _collection.Values[Start + index]);
        }
    }

    /// <summary>
    /// Moves the span by the specified offset.
    /// </summary>
    /// <param name="offset">The number of positions to move the span.</param>
    /// <returns>True if the move was successful; otherwise, false.</returns>
    public bool Move(int offset)
    {
        var start = Start + offset;
        if (start < 0 || start + Count > _collection.Count)
            return false;

        Start = start;

        return true;
    }

    /// <summary>
    /// Returns an enumerator that iterates through the span of key-value pairs.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the span.</returns>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        for (var i = 0; i < Count; i++)
            yield return KeyValuePair.Create(_collection.Keys[Start + i], _collection.Values[Start + i]);
    }

    /// <summary>
    /// Returns an enumerator that iterates through the span of key-value pairs.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the span.</returns>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
