using System;
using System.Threading;
using Annium.Core.DependencyInjection;
using Annium.Extensions.Pooling.Internal.Loaders;
using Annium.Extensions.Pooling.Internal.Storages;

namespace Annium.Extensions.Pooling.Internal;

internal class ObjectPool<T> : IObjectPool<T>
    where T : notnull
{
    private readonly Lock _locker = new();
    private readonly ILoader<T> _loader;
    private readonly IStorage<T> _storage;
    private readonly Semaphore _semaphore;

    public ObjectPool(IServiceProvider sp, ObjectPoolConfig<T> config)
    {
        _storage = StorageFactory.Create<T>(config.StorageMode, config.Capacity);
        _loader = LoaderFactory.Create(config.LoadMode, sp.Resolve<T>, _storage);
        _semaphore = new Semaphore(config.Capacity, config.Capacity);
    }

    public T Get()
    {
        _semaphore.WaitOne();
        lock (_locker)
            return _loader.Get();
    }

    public void Return(T item)
    {
        lock (_locker)
            _storage.Return(item);
        _semaphore.Release();
    }

    #region IDisposable support

    private bool _disposedValue; // To detect redundant calls

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

    public void Dispose()
    {
        Dispose(true);
    }

    #endregion
}
