using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Annium.Collections.Generic;

/// <summary>
/// Represents a fixed-size queue with indexed access to its elements.
/// </summary>
/// <typeparam name="T">The type of the elements in the queue.</typeparam>
public class FixedIndexedQueue<T> : IFixedIndexedQueue<T>
{
    /// <summary>
    /// Gets the capacity of the queue.
    /// </summary>
    public int Capacity { get; }

    /// <summary>
    /// Gets the number of elements contained in the queue.
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// The maximum index in the queue.
    /// </summary>
    private readonly int _maxIndex;

    /// <summary>
    /// The underlying array storing the elements.
    /// </summary>
    private readonly T[] _data;

    /// <summary>
    /// The current index in the queue.
    /// </summary>
    private int _index;

    /// <summary>
    /// Initializes a new instance of the <see cref="FixedIndexedQueue{T}"/> class with the specified capacity.
    /// </summary>
    /// <param name="capacity">The maximum number of elements the queue can hold.</param>
    public FixedIndexedQueue(int capacity)
    {
        Capacity = capacity;
        _maxIndex = capacity - 1;
        _data = new T[capacity];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FixedIndexedQueue{T}"/> class with the specified collection.
    /// </summary>
    /// <param name="data">The collection to initialize the queue with.</param>
    public FixedIndexedQueue(IReadOnlyCollection<T> data)
    {
        Capacity = Count = data.Count;
        _maxIndex = data.Count - 1;
        _data = data.ToArray();
    }

    /// <summary>
    /// Gets the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get.</param>
    /// <returns>The element at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the index is out of range.</exception>
    public T this[int index]
    {
        get
        {
            if (index < 0 || index > _maxIndex)
                throw new ArgumentOutOfRangeException($"Index {index} is out of range [0;{_maxIndex}]");

            return _data[(_index + index) % Capacity];
        }
    }

    /// <summary>
    /// Adds an item to the queue.
    /// </summary>
    /// <param name="item">The item to add.</param>
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

    /// <summary>
    /// Returns an enumerator that iterates through the queue.
    /// </summary>
    /// <returns>An enumerator for the queue.</returns>
    public IEnumerator<T> GetEnumerator()
    {
        for (var i = 0; i < Count; i++)
            yield return _data[(_index + i) % Capacity];
    }

    /// <summary>
    /// Returns an enumerator that iterates through the queue.
    /// </summary>
    /// <returns>An enumerator for the queue.</returns>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// Defines a fixed-size queue with indexed access to its elements.
/// </summary>
/// <typeparam name="T">The type of the elements in the queue.</typeparam>
public interface IFixedIndexedQueue<T> : IReadOnlyList<T>
{
    /// <summary>
    /// Gets the capacity of the queue.
    /// </summary>
    int Capacity { get; }

    /// <summary>
    /// Adds an item to the queue.
    /// </summary>
    /// <param name="item">The item to add.</param>
    void Add(T item);
}
