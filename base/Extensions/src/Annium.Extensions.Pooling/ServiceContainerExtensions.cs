using Annium.Extensions.Pooling;
using Annium.Extensions.Pooling.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
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