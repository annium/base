using System;
using Annium.Extensions.Pooling.Internal.Storages;

namespace Annium.Extensions.Pooling.Internal.Loaders;

internal static class LoaderFactory
{
    public static ILoader<T> Create<T>(
        PoolLoadMode mode,
        Func<T> factory,
        IStorage<T> storage
    ) => mode switch
    {
        PoolLoadMode.Eager => new EagerLoader<T>(factory, storage),
        PoolLoadMode.Lazy  => new LazyLoader<T>(factory, storage),
        _                  => throw new NotImplementedException($"Unsupported load mode {mode}"),
    };
}