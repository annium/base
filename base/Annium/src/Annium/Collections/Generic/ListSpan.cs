using System;
using System.Collections;
using System.Collections.Generic;

namespace Annium.Collections.Generic;

public record ListSpan<T> : IListSpan<T>
{
    public int Count { get; }
    public int Start { get; private set; }
    public int End => Start + Count - 1;

    private readonly IReadOnlyList<T> _collection;

    public ListSpan(
        IReadOnlyList<T> collection,
        int start,
        int count
    )
    {
        if (start < 0 || start + count > collection.Count)
            throw new ArgumentOutOfRangeException($"Invalid span at {start} with length {count} for collection of size {collection.Count}");

        _collection = collection;
        Start = start;
        Count = count;
    }

    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException($"Index {index} if out of range [0;{Count}]");

            return _collection[Start + index];
        }
    }

    public bool Move(int offset)
    {
        var start = Start + offset;
        if (start < 0 || start + Count > _collection.Count)
            return false;

        Start = start;

        return true;
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (var i = 0; i < Count; i++)
            yield return _collection[Start + i];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public interface IListSpan<out T> : IReadOnlyIndexedSpan<T>
{
    bool Move(int offset);
}