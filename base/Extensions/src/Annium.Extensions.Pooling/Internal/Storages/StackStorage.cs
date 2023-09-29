using System;
using System.Collections.Generic;

namespace Annium.Extensions.Pooling.Internal.Storages;

internal class StackStorage<T> : StorageBase<T>, IStorage<T>
{
    private readonly Stack<T> _freeItems;
    private readonly List<T> _usedItems;

    public StackStorage(int capacity) : base(capacity)
    {
        _freeItems = new Stack<T>(capacity);
        _usedItems = new List<T>(capacity);
    }

    protected override void Register(T item) => _freeItems.Push(item);

    protected override T Acquire()
    {
        var item = _freeItems.Pop();
        _usedItems.Add(item);

        return item;
    }

    protected override bool Release(T item)
    {
        var released = _usedItems.Remove(item);
        if (!released)
            return false;

        _freeItems.Push(item);

        return true;
    }

    protected override void DisposeInternal()
    {
        foreach (var item in _freeItems)
            (item as IDisposable)!.Dispose();
        _freeItems.Clear();

        foreach (var item in _usedItems)
            (item as IDisposable)!.Dispose();
        _usedItems.Clear();
    }
}