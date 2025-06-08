using System;
using System.Collections.Generic;

namespace Annium.Extensions.Pooling.Internal.Storages;

/// <summary>
/// Implements FIFO (First-In-First-Out) storage for pooled objects using queue semantics.
/// Objects are retrieved in the same order they were added to the pool.
/// </summary>
/// <typeparam name="T">The type of objects stored in the pool.</typeparam>
internal class QueueStorage<T> : StorageBase<T>, IStorage<T>
{
    /// <summary>
    /// Queue containing objects available for use.
    /// </summary>
    private readonly Queue<T> _freeItems;

    /// <summary>
    /// List containing objects currently in use.
    /// </summary>
    private readonly List<T> _usedItems;

    /// <summary>
    /// Initializes a new instance of the QueueStorage class with the specified capacity.
    /// </summary>
    /// <param name="capacity">The maximum number of objects this storage can hold.</param>
    public QueueStorage(int capacity)
        : base(capacity)
    {
        _freeItems = new Queue<T>(capacity);
        _usedItems = new List<T>(capacity);
    }

    /// <summary>
    /// Registers an object by adding it to the end of the free items queue.
    /// </summary>
    /// <param name="item">The object to register.</param>
    protected override void Register(T item) => _freeItems.Enqueue(item);

    /// <summary>
    /// Acquires the oldest object from the free items queue and moves it to the used items list.
    /// </summary>
    /// <returns>The object that was acquired.</returns>
    protected override T Acquire()
    {
        var item = _freeItems.Dequeue();
        _usedItems.Add(item);

        return item;
    }

    /// <summary>
    /// Releases an object back to the free items queue if it was found in the used items list.
    /// </summary>
    /// <param name="item">The object to release.</param>
    /// <returns>True if the object was successfully released; otherwise, false.</returns>
    protected override bool Release(T item)
    {
        var released = _usedItems.Remove(item);
        if (!released)
            return false;

        _freeItems.Enqueue(item);

        return true;
    }

    /// <summary>
    /// Disposes all objects in both free and used collections if they implement IDisposable.
    /// </summary>
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
