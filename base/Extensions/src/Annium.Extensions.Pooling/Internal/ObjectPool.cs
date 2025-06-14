using System;
using System.Threading;
using Annium.Core.DependencyInjection;
using Annium.Extensions.Pooling.Internal.Loaders;
using Annium.Extensions.Pooling.Internal.Storages;

namespace Annium.Extensions.Pooling.Internal;

/// <summary>
/// Thread-safe object pool implementation that manages a fixed number of reusable objects.
/// Uses configurable loading and storage strategies for optimal performance.
/// </summary>
/// <typeparam name="T">The type of objects managed by the pool. Must be non-null.</typeparam>
internal class ObjectPool<T> : IObjectPool<T>
    where T : notnull
{
    /// <summary>
    /// Lock object for thread synchronization.
    /// </summary>
    private readonly Lock _locker = new();

    /// <summary>
    /// Loader responsible for creating and retrieving objects from storage.
    /// </summary>
    private readonly ILoader<T> _loader;

    /// <summary>
    /// Storage container that manages the actual pooled objects.
    /// </summary>
    private readonly IStorage<T> _storage;

    /// <summary>
    /// Semaphore that controls access to the pool based on capacity.
    /// </summary>
    private readonly Semaphore _semaphore;

    /// <summary>
    /// Initializes a new instance of the ObjectPool class with the specified configuration.
    /// </summary>
    /// <param name="sp">Service provider used to resolve object instances.</param>
    /// <param name="config">Configuration that defines pool behavior and capacity.</param>
    public ObjectPool(IServiceProvider sp, ObjectPoolConfig<T> config)
    {
        _storage = StorageFactory.Create<T>(config.StorageMode, config.Capacity);
        _loader = LoaderFactory.Create(config.LoadMode, sp.Resolve<T>, _storage);
        _semaphore = new Semaphore(config.Capacity, config.Capacity);
    }

    /// <summary>
    /// Gets an object from the pool. Blocks if no objects are available until one becomes free.
    /// </summary>
    /// <returns>An object from the pool.</returns>
    public T Get()
    {
        _semaphore.WaitOne();
        lock (_locker)
            return _loader.Get();
    }

    /// <summary>
    /// Returns an object to the pool, making it available for reuse.
    /// </summary>
    /// <param name="item">The object to return to the pool.</param>
    public void Return(T item)
    {
        lock (_locker)
            _storage.Return(item);
        _semaphore.Release();
    }

    #region IDisposable support

    /// <summary>
    /// Flag to detect redundant disposal calls.
    /// </summary>
    private bool _disposedValue;

    /// <summary>
    /// Releases the unmanaged resources used by the ObjectPool and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposedValue)
            return;

        if (disposing)
        {
            if (typeof(IDisposable).IsAssignableFrom(typeof(T)))
                _storage.Dispose();
            _semaphore.Close();
        }

        _disposedValue = true;
    }

    /// <summary>
    /// Releases all resources used by the ObjectPool.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
    }

    #endregion
}
