using System;
using System.Collections;
using System.Collections.Generic;

namespace Annium.Collections.Generic;

/// <summary>
/// Represents a span of elements from a read-only list, with the ability to move the span's position.
/// </summary>
/// <typeparam name="T">The type of the elements in the span.</typeparam>
public record ListSpan<T> : IListSpan<T>
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
    /// The underlying collection that this span references.
    /// </summary>
    private readonly IReadOnlyList<T> _collection;

    /// <summary>
    /// Initializes a new instance of the <see cref="ListSpan{T}"/> class.
    /// </summary>
    /// <param name="collection">The source collection to create a span from.</param>
    /// <param name="start">The starting index of the span.</param>
    /// <param name="count">The number of elements in the span.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the span parameters are invalid.</exception>
    public ListSpan(IReadOnlyList<T> collection, int start, int count)
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
    /// Gets the element at the specified index within the span.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get.</param>
    /// <returns>The element at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the index is out of range.</exception>
    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException($"Index {index} if out of range [0;{Count}]");

            return _collection[Start + index];
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
    /// Returns an enumerator that iterates through the span.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the span.</returns>
    public IEnumerator<T> GetEnumerator()
    {
        for (var i = 0; i < Count; i++)
            yield return _collection[Start + i];
    }

    /// <summary>
    /// Returns an enumerator that iterates through the span.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the span.</returns>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// Defines a span of elements from a read-only list with the ability to move the span's position.
/// </summary>
/// <typeparam name="T">The type of the elements in the span.</typeparam>
public interface IListSpan<out T> : IReadOnlyIndexedSpan<T>
{
    /// <summary>
    /// Moves the span by the specified offset.
    /// </summary>
    /// <param name="offset">The number of positions to move the span.</param>
    /// <returns>True if the move was successful; otherwise, false.</returns>
    bool Move(int offset);
}
