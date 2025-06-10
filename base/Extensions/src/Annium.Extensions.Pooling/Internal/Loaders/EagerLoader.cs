using System;
using Annium.Extensions.Pooling.Internal.Storages;

namespace Annium.Extensions.Pooling.Internal.Loaders;

/// <summary>
/// Implements an eager loading strategy that prioritizes pre-creating objects up to the pool's capacity before serving them.
/// This loader ensures objects are created and added to storage before they are needed for better performance predictability.
/// </summary>
/// <typeparam name="T">The type of objects to load from the pool.</typeparam>
internal class EagerLoader<T> : ILoader<T>
{
    /// <summary>
    /// Factory function to create new instances of type T.
    /// </summary>
    private readonly Func<T> _factory;

    /// <summary>
    /// Storage container for managing pooled objects.
    /// </summary>
    private readonly IStorage<T> _storage;

    /// <summary>
    /// Initializes a new instance of the EagerLoader class.
    /// </summary>
    /// <param name="factory">Factory function to create new instances of type T.</param>
    /// <param name="storage">Storage container for managing pooled objects.</param>
    public EagerLoader(Func<T> factory, IStorage<T> storage)
    {
        _factory = factory;
        _storage = storage;
    }

    /// <summary>
    /// Gets an object from the pool using eager loading strategy. If the pool is not at full capacity,
    /// creates a new object first before serving any existing objects.
    /// </summary>
    /// <returns>An object of type T from the pool.</returns>
    public T Get()
    {
        // if not all created yet - create first
        if (_storage.Free + _storage.Used < _storage.Capacity)
        {
            var item = _factory();
            _storage.Add(item);
        }

        return _storage.Get();
    }
}
