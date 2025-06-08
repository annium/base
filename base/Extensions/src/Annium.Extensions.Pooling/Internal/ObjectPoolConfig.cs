using System;

namespace Annium.Extensions.Pooling.Internal;

/// <summary>
/// Configuration record for object pools that defines capacity and behavior modes.
/// </summary>
/// <typeparam name="T">The type of objects the pool will manage.</typeparam>
public record ObjectPoolConfig<T>
{
    /// <summary>
    /// Gets the maximum number of objects the pool can hold.
    /// </summary>
    public int Capacity { get; }

    /// <summary>
    /// Gets the loading mode that determines when objects are created.
    /// </summary>
    public PoolLoadMode LoadMode { get; }

    /// <summary>
    /// Gets the storage mode that determines the retrieval strategy for objects.
    /// </summary>
    public PoolStorageMode StorageMode { get; }

    /// <summary>
    /// Initializes a new instance of the ObjectPoolConfig record.
    /// </summary>
    /// <param name="capacity">The maximum number of objects the pool can hold. Must be greater than zero.</param>
    /// <param name="loadMode">The loading mode that determines when objects are created.</param>
    /// <param name="storageMode">The storage mode that determines the retrieval strategy for objects.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when capacity is less than or equal to zero.</exception>
    public ObjectPoolConfig(int capacity, PoolLoadMode loadMode, PoolStorageMode storageMode)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(capacity),
                capacity,
                $"Capacity = '{nameof(capacity)}', but must be greater than zero."
            );

        Capacity = capacity;
        LoadMode = loadMode;
        StorageMode = storageMode;
    }
}
