using System;

namespace Annium.Extensions.Pooling.Internal;

public record ObjectPoolConfig<T>
{
    public int Capacity { get; }
    public PoolLoadMode LoadMode { get; }
    public PoolStorageMode StorageMode { get; }

    public ObjectPoolConfig(
        int capacity,
        PoolLoadMode loadMode,
        PoolStorageMode storageMode
    )
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), capacity, $"Capacity = '{nameof(capacity)}', but must be greater than zero.");

        Capacity = capacity;
        LoadMode = loadMode;
        StorageMode = storageMode;
    }
}