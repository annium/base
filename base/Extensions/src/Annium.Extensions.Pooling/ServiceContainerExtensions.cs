using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Descriptors;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Extensions.Pooling.Internal;

namespace Annium.Extensions.Pooling;

/// <summary>
/// Extension methods for configuring object pooling and caching services in the dependency injection container.
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Registers an object cache with the specified provider in the service container.
    /// </summary>
    /// <typeparam name="TKey">The type of keys used to identify cached objects. Must be non-null.</typeparam>
    /// <typeparam name="TValue">The type of values stored in the cache. Must be a reference type.</typeparam>
    /// <typeparam name="TProvider">The provider type that manages object lifecycle operations.</typeparam>
    /// <param name="container">The service container to register services in.</param>
    /// <param name="lifetime">The service lifetime for the cache and provider.</param>
    /// <returns>The service container for method chaining.</returns>
    public static IServiceContainer AddObjectCache<TKey, TValue, TProvider>(
        this IServiceContainer container,
        ServiceLifetime lifetime
    )
        where TKey : notnull
        where TValue : class
        where TProvider : ObjectCacheProvider<TKey, TValue>
    {
        container.Add<ObjectCacheProvider<TKey, TValue>, TProvider>().In(lifetime);
        container.Add<IObjectCache<TKey, TValue>, ObjectCache<TKey, TValue>>().In(lifetime);

        return container;
    }

    /// <summary>
    /// Registers an object pool with the specified configuration in the service container.
    /// </summary>
    /// <typeparam name="T">The type of objects managed by the pool. Must be non-null.</typeparam>
    /// <param name="container">The service container to register services in.</param>
    /// <param name="capacity">The maximum number of objects the pool can hold.</param>
    /// <param name="lifetime">The service lifetime for the pool.</param>
    /// <param name="loadMode">The loading mode that determines when objects are created. Defaults to Lazy.</param>
    /// <param name="storageMode">The storage mode that determines the retrieval strategy. Defaults to FIFO.</param>
    /// <returns>The service container for method chaining.</returns>
    public static IServiceContainer AddObjectPool<T>(
        this IServiceContainer container,
        int capacity,
        ServiceLifetime lifetime,
        PoolLoadMode loadMode = PoolLoadMode.Lazy,
        PoolStorageMode storageMode = PoolStorageMode.Fifo
    )
        where T : notnull
    {
        container.Add<IObjectPool<T>, ObjectPool<T>>().In(lifetime);
        container.Add(new ObjectPoolConfig<T>(capacity, loadMode, storageMode)).AsSelf().Singleton();

        return container;
    }
}
