using System;
using System.Collections;
using System.Collections.Generic;

namespace Annium.Collections.Generic;

public record SortedListSpan<TKey, TValue> : ISortedListSpan<TKey, TValue>
    where TKey : notnull
{
    public int Count { get; }
    public int Start { get; private set; }
    public int End => Start + Count - 1;

    private readonly SortedList<TKey, TValue> _collection;

    public SortedListSpan(
        SortedList<TKey, TValue> collection,
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

    public bool Move(int offset)
    {
        var start = Start + offset;
        if (start < 0 || start + Count > _collection.Count)
            return false;

        Start = start;

        return true;
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        for (var i = 0; i < Count; i++)
        {
            var key = _collection.Keys[Start + i];

            yield return KeyValuePair.Create(key, _collection[key]);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public interface ISortedListSpan<TKey, TValue> : IReadOnlyIndexedSpan<KeyValuePair<TKey, TValue>>
    where TKey : notnull
{
    bool Move(int offset);
}