using System;
using Annium.Extensions.Pooling.Internal.Storages;

namespace Annium.Extensions.Pooling.Internal.Loaders;

/// <summary>
/// Implements a lazy loading strategy that prioritizes reusing existing objects before creating new ones.
/// This loader only creates new objects when no free objects are available and capacity allows.
/// </summary>
/// <typeparam name="T">The type of objects to load from the pool.</typeparam>
internal class LazyLoader<T> : ILoader<T>
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
    /// Initializes a new instance of the LazyLoader class.
    /// </summary>
    /// <param name="factory">Factory function to create new instances of type T.</param>
    /// <param name="storage">Storage container for managing pooled objects.</param>
    public LazyLoader(Func<T> factory, IStorage<T> storage)
    {
        _factory = factory;
        _storage = storage;
    }

    /// <summary>
    /// Gets an object from the pool using lazy loading strategy. Returns an existing free object if available,
    /// otherwise creates a new object if capacity allows.
    /// </summary>
    /// <returns>An object of type T from the pool.</returns>
    public T Get()
    {
        // if any free - try use first
        if (_storage.Free > 0)
            return _storage.Get();

        // if not all created yet - create
        if (_storage.Used < _storage.Capacity)
        {
            var item = _factory();
            _storage.Add(item);
        }

        return _storage.Get();
    }
}
