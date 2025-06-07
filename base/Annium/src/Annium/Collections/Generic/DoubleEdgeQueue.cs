using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Annium.Collections.Generic;

/// <summary>
/// Represents a double-ended queue (deque) with configurable directionality.
/// </summary>
/// <typeparam name="T">The type of elements in the queue.</typeparam>
public class DoubleEdgeQueue<T> : IDoubleEdgeQueue<T>
{
    /// <summary>
    /// Indicates whether the queue operates in direct mode.
    /// </summary>
    private readonly bool _isDirect;

    /// <summary>
    /// Gets the number of elements contained in the queue.
    /// </summary>
    public int Count => _entries.Count;

    /// <summary>
    /// Gets the first element in the queue.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the queue is empty.</exception>
    public T First =>
        _entries.Count > 0 ? _entries.First!.Value : throw new InvalidOperationException("queue is empty");

    /// <summary>
    /// Gets the last element in the queue.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the queue is empty.</exception>
    public T Last => _entries.Count > 0 ? _entries.Last!.Value : throw new InvalidOperationException("queue is empty");

    /// <summary>
    /// The underlying linked list storing the elements.
    /// </summary>
    private readonly LinkedList<T> _entries = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DoubleEdgeQueue{T}"/> class with the specified entries and directionality.
    /// </summary>
    /// <param name="entries">The initial elements to add to the queue.</param>
    /// <param name="isDirect">If true, operates in direct mode; otherwise, in reverse mode.</param>
    public DoubleEdgeQueue(IEnumerable<T> entries, bool isDirect)
    {
#pragma warning disable IDE0306
        _entries = new LinkedList<T>(entries);
#pragma warning restore IDE0306
        _isDirect = isDirect;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DoubleEdgeQueue{T}"/> class with the specified directionality.
    /// </summary>
    /// <param name="isDirect">If true, operates in direct mode; otherwise, in reverse mode.</param>
    public DoubleEdgeQueue(bool isDirect)
    {
        _isDirect = isDirect;
    }

    /// <summary>
    /// Adds an element to the front of the queue.
    /// </summary>
    /// <param name="value">The element to add.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddFirst(T value)
    {
        if (_isDirect)
            _entries.AddFirst(value);
        else
            _entries.AddLast(value);
    }

    /// <summary>
    /// Adds an element to the end of the queue.
    /// </summary>
    /// <param name="value">The element to add.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddLast(T value)
    {
        if (_isDirect)
            _entries.AddLast(value);
        else
            _entries.AddFirst(value);
    }

    /// <summary>
    /// Removes the first element from the queue.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveFirst()
    {
        if (_isDirect)
            _entries.RemoveFirst();
        else
            _entries.RemoveLast();
    }

    /// <summary>
    /// Removes the last element from the queue.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveLast()
    {
        if (_isDirect)
            _entries.RemoveLast();
        else
            _entries.RemoveFirst();
    }

    /// <summary>
    /// Returns an enumerator that iterates through the queue.
    /// </summary>
    /// <returns>An enumerator for the queue.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerator<T> GetEnumerator() => _entries.GetEnumerator();

    /// <summary>
    /// Returns an enumerator that iterates through the queue.
    /// </summary>
    /// <returns>An enumerator for the queue.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator IEnumerable.GetEnumerator() => _entries.GetEnumerator();
}
