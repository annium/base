using System;
using System.Collections.Generic;

namespace Annium.Extensions.Pooling.Internal.Storages;

internal class QueueStorage<T> : StorageBase<T>, IStorage<T>
{
    private readonly Queue<T> _freeItems;
    private readonly List<T> _usedItems;

    public QueueStorage(int capacity) : base(capacity)
    {
        _freeItems = new Queue<T>(capacity);
        _usedItems = new List<T>(capacity);
    }

    protected override void Register(T item) => _freeItems.Enqueue(item);

    protected override T Acquire()
    {
        var item = _freeItems.Dequeue();
        _usedItems.Add(item);

        return item;
    }

    protected override bool Release(T item)
    {
        var released = _usedItems.Remove(item);
        if (!released)
            return false;

        _freeItems.Enqueue(item);

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