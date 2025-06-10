using System;

namespace Annium.Extensions.Pooling.Internal.Storages;

/// <summary>
/// Factory class for creating appropriate storage instances based on the specified storage mode.
/// </summary>
internal static class StorageFactory
{
    /// <summary>
    /// Creates a storage instance based on the specified storage mode.
    /// </summary>
    /// <typeparam name="T">The type of objects the storage will handle.</typeparam>
    /// <param name="mode">The storage mode that determines the retrieval strategy (FIFO or LIFO).</param>
    /// <param name="capacity">The maximum number of objects the storage can hold.</param>
    /// <returns>A storage instance implementing the specified retrieval strategy.</returns>
    /// <exception cref="NotImplementedException">Thrown when an unsupported storage mode is specified.</exception>
    public static IStorage<T> Create<T>(PoolStorageMode mode, int capacity) =>
        mode switch
        {
            PoolStorageMode.Fifo => new QueueStorage<T>(capacity),
            PoolStorageMode.Lifo => new StackStorage<T>(capacity),
            _ => throw new NotImplementedException($"Unsupported storage mode {mode}"),
        };
}
