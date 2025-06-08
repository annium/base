using System;

namespace Annium.Extensions.Pooling.Internal.Storages;

/// <summary>
/// Provides a base implementation for storage containers that manage pooled objects.
/// Handles common functionality like capacity tracking and state management.
/// </summary>
/// <typeparam name="T">The type of objects stored in the pool.</typeparam>
internal abstract class StorageBase<T> : IDisposable
{
    /// <summary>
    /// Gets the maximum number of objects this storage can hold.
    /// </summary>
    public int Capacity { get; }

    /// <summary>
    /// Gets the number of objects currently available for use.
    /// </summary>
    public int Free { get; private set; }

    /// <summary>
    /// Gets the number of objects currently in use.
    /// </summary>
    public int Used { get; private set; }

    /// <summary>
    /// Initializes a new instance of the StorageBase class with the specified capacity.
    /// </summary>
    /// <param name="capacity">The maximum number of objects this storage can hold.</param>
    protected StorageBase(int capacity)
    {
        Capacity = capacity;
    }

    /// <summary>
    /// Adds a new object to the storage as available for use.
    /// </summary>
    /// <param name="item">The object to add to the storage.</param>
    /// <exception cref="InvalidOperationException">Thrown when storage capacity is reached.</exception>
    public void Add(T item)
    {
        if (Free == Capacity)
            throw new InvalidOperationException("Storage capacity reached");

        Register(item);
        ++Free;
    }

    /// <summary>
    /// Gets an available object from the storage and marks it as used.
    /// </summary>
    /// <returns>An object from the storage.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no free objects are available.</exception>
    public T Get()
    {
        if (Free == 0)
            throw new InvalidOperationException("No free slots.");

        var item = Acquire();
        --Free;
        ++Used;

        return item;
    }

    /// <summary>
    /// Returns a used object back to the storage, making it available for reuse.
    /// </summary>
    /// <param name="item">The object to return to the storage.</param>
    /// <exception cref="InvalidOperationException">Thrown when the item is not registered in storage.</exception>
    public void Return(T item)
    {
        if (!Release(item))
            throw new InvalidOperationException("Item is not registered in storage.");

        ++Free;
        --Used;
    }

    /// <summary>
    /// Registers an object in the storage implementation's internal data structure.
    /// </summary>
    /// <param name="item">The object to register.</param>
    protected abstract void Register(T item);

    /// <summary>
    /// Acquires an object from the storage implementation's internal data structure.
    /// </summary>
    /// <returns>An object from the storage.</returns>
    protected abstract T Acquire();

    /// <summary>
    /// Releases an object back to the storage implementation's internal data structure.
    /// </summary>
    /// <param name="item">The object to release.</param>
    /// <returns>True if the object was successfully released; otherwise, false.</returns>
    protected abstract bool Release(T item);

    /// <summary>
    /// Performs storage-specific disposal operations.
    /// </summary>
    protected abstract void DisposeInternal();

    /// <summary>
    /// Releases all resources used by the storage.
    /// </summary>
    public void Dispose()
    {
        DisposeInternal();
    }
}
