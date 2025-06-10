using System;

namespace Annium.Extensions.Pooling.Internal.Storages;

/// <summary>
/// Defines a contract for storage containers that manage pooled objects with different retrieval strategies.
/// </summary>
/// <typeparam name="T">The type of objects stored in the pool.</typeparam>
internal interface IStorage<T> : IDisposable
{
    /// <summary>
    /// Gets the maximum number of objects this storage can hold.
    /// </summary>
    int Capacity { get; }

    /// <summary>
    /// Gets the number of objects currently available for use.
    /// </summary>
    int Free { get; }

    /// <summary>
    /// Gets the number of objects currently in use.
    /// </summary>
    int Used { get; }

    /// <summary>
    /// Adds a new object to the storage as available for use.
    /// </summary>
    /// <param name="item">The object to add to the storage.</param>
    void Add(T item);

    /// <summary>
    /// Gets an available object from the storage and marks it as used.
    /// </summary>
    /// <returns>An object from the storage.</returns>
    T Get();

    /// <summary>
    /// Returns a used object back to the storage, making it available for reuse.
    /// </summary>
    /// <param name="item">The object to return to the storage.</param>
    void Return(T item);
}
