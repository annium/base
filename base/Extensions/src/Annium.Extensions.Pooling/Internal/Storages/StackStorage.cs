using System;
using System.Collections.Generic;

namespace Annium.Extensions.Pooling.Internal.Storages;

/// <summary>
/// Implements LIFO (Last-In-First-Out) storage for pooled objects using stack semantics.
/// Objects are retrieved in reverse order from which they were added to the pool.
/// </summary>
/// <typeparam name="T">The type of objects stored in the pool.</typeparam>
internal class StackStorage<T> : StorageBase<T>, IStorage<T>
{
    /// <summary>
    /// Stack containing objects available for use.
    /// </summary>
    private readonly Stack<T> _freeItems;

    /// <summary>
    /// List containing objects currently in use.
    /// </summary>
    private readonly List<T> _usedItems;

    /// <summary>
    /// Initializes a new instance of the StackStorage class with the specified capacity.
    /// </summary>
    /// <param name="capacity">The maximum number of objects this storage can hold.</param>
    public StackStorage(int capacity)
        : base(capacity)
    {
        _freeItems = new Stack<T>(capacity);
        _usedItems = new List<T>(capacity);
    }

    /// <summary>
    /// Registers an object by pushing it onto the top of the free items stack.
    /// </summary>
    /// <param name="item">The object to register.</param>
    protected override void Register(T item) => _freeItems.Push(item);

    /// <summary>
    /// Acquires the most recently added object from the free items stack and moves it to the used items list.
    /// </summary>
    /// <returns>The object that was acquired.</returns>
    protected override T Acquire()
    {
        var item = _freeItems.Pop();
        _usedItems.Add(item);

        return item;
    }

    /// <summary>
    /// Releases an object back to the free items stack if it was found in the used items list.
    /// </summary>
    /// <param name="item">The object to release.</param>
    /// <returns>True if the object was successfully released; otherwise, false.</returns>
    protected override bool Release(T item)
    {
        var released = _usedItems.Remove(item);
        if (!released)
            return false;

        _freeItems.Push(item);

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
