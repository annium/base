using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Annium.Collections.Generic;

public class DoubleEdgeQueue<T> : IDoubleEdgeQueue<T>
{
    private readonly bool _isDirect;
    public int Count => _entries.Count;
    public T First => _entries.Count > 0 ? _entries.First!.Value : throw new InvalidOperationException("queue is empty");
    public T Last => _entries.Count > 0 ? _entries.Last!.Value : throw new InvalidOperationException("queue is empty");
    private readonly LinkedList<T> _entries = new();

    public DoubleEdgeQueue(bool isDirect)
    {
        _isDirect = isDirect;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddFirst(T value)
    {
        if (_isDirect)
            _entries.AddFirst(value);
        else
            _entries.AddLast(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddLast(T value)
    {
        if (_isDirect)
            _entries.AddLast(value);
        else
            _entries.AddFirst(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveFirst()
    {
        if (_isDirect)
            _entries.RemoveFirst();
        else
            _entries.RemoveLast();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveLast()
    {
        if (_isDirect)
            _entries.RemoveLast();
        else
            _entries.RemoveFirst();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerator<T> GetEnumerator() => _entries.GetEnumerator();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator IEnumerable.GetEnumerator() => _entries.GetEnumerator();
}