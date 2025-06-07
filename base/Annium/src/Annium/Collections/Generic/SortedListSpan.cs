using System;
using System.Collections;
using System.Collections.Generic;

namespace Annium.Collections.Generic;

/// <summary>
/// Represents a span of key-value pairs from a sorted list, with the ability to move the span's position.
/// </summary>
/// <typeparam name="TKey">The type of the keys in the sorted list.</typeparam>
/// <typeparam name="TValue">The type of the values in the sorted list.</typeparam>
public record SortedListSpan<TKey, TValue> : ISortedListSpan<TKey, TValue>
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
        if (start < 0 || start + count > collection.Count)
            throw new ArgumentOutOfRangeException(
                $"Invalid span at {start} with length {count} for collection of size {collection.Count}"
            );

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
                throw new ArgumentOutOfRangeException($"Index {index} if out of range [0;{Count}]");

            var key = _collection.Keys[Start + index];

            return KeyValuePair.Create(key, _collection[key]);
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
        {
            var key = _collection.Keys[Start + i];

            yield return KeyValuePair.Create(key, _collection[key]);
        }
    }

    /// <summary>
    /// Returns an enumerator that iterates through the span of key-value pairs.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the span.</returns>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

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
