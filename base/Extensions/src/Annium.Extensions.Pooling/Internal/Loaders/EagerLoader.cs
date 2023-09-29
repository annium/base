using System;
using Annium.Extensions.Pooling.Internal.Storages;

namespace Annium.Extensions.Pooling.Internal.Loaders;

internal class EagerLoader<T> : ILoader<T>
{
    private readonly Func<T> _factory;
    private readonly IStorage<T> _storage;

    public EagerLoader(
        Func<T> factory,
        IStorage<T> storage
    )
    {
        _factory = factory;
        _storage = storage;
    }

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