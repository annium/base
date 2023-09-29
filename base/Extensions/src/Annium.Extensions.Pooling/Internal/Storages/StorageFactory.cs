using System;

namespace Annium.Extensions.Pooling.Internal.Storages;

internal static class StorageFactory
{
    public static IStorage<T> Create<T>(PoolStorageMode mode, int capacity) => mode switch
    {
        PoolStorageMode.Fifo => new QueueStorage<T>(capacity),
        PoolStorageMode.Lifo => new StackStorage<T>(capacity),
        _                    => throw new NotImplementedException($"Unsupported storage mode {mode}"),
    };
}