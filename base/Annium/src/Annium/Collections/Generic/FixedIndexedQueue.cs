using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Annium.Collections.Generic;

public class FixedIndexedQueue<T> : IFixedIndexedQueue<T>
{
    public int Capacity { get; }
    public int Count { get; private set; }
    private readonly int _maxIndex;
    private readonly T[] _data;
    private int _index;

    public FixedIndexedQueue(int capacity)
    {
        Capacity = capacity;
        _maxIndex = capacity - 1;
        _data = new T[capacity];
    }

    public FixedIndexedQueue(IReadOnlyCollection<T> data)
    {
        Capacity = Count = data.Count;
        _maxIndex = data.Count - 1;
        _data = data.ToArray();
    }

    public T this[int index]
    {
        get
        {
            if (index < 0 || index > _maxIndex)
                throw new ArgumentOutOfRangeException($"Index {index} is out of range [0;{_maxIndex}]");

            return _data[(_index + index) % Capacity];
        }
    }

    public void Add(T item)
    {
        if (Count < Capacity)
        {
            _data[Count] = item;
            Count++;
        }
        else
        {
            _data[_index] = item;
            _index = _index == _maxIndex ? 0 : _index + 1;
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (var i = 0; i < Count; i++)
            yield return _data[(_index + i) % Capacity];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public interface IFixedIndexedQueue<T> : IReadOnlyList<T>
{
    int Capacity { get; }
    void Add(T item);
}