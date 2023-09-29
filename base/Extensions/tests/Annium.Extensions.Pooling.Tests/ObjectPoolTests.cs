using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Pooling.Tests;

public class ObjectPoolTests
{
    private const int Capacity = 5;
    private const int Jobs = 20;
    private const string Created = "Created";
    private const string Action = "ExecuteAction";
    private const string Disposed = "Disposed";

    [Fact]
    public async Task ObjectPool_Eager_FIFO_Works()
    {
        // arrange
        var (pool, logs) = CreatePool(PoolLoadMode.Eager, PoolStorageMode.Fifo);

        // act
        await Run(pool);
        pool.Dispose();

        // assert
        // as eager - all workers Created
        logs.Where(x => x.Contains(Created)).Has(Capacity);
        // all job is done
        logs.Where(x => x.Contains(Action)).Has(Jobs);
        Enumerable.Range(0, Jobs).All(i => logs.Any(e => e.Contains($"{Action} {i}"))).IsTrue();
        // all workers are Disposed
        logs.Where(x => x.Contains(Disposed)).Has(Capacity);
    }

    [Fact]
    public async Task ObjectPool_Lazy_LIFO_Works()
    {
        // arrange
        var (pool, logs) = CreatePool(PoolLoadMode.Lazy, PoolStorageMode.Lifo);

        // act
        await Run(pool);
        pool.Dispose();

        // assert
        // all job is done
        Enumerable.Range(0, Jobs).All(i => logs.Any(e => e.Contains($"{Action} {i}"))).IsTrue();
        // all workers are Disposed
        (logs.Count(x => x.Contains(Disposed)) == logs.Count(x => x.Contains(Created))).IsTrue();
    }

    private (IObjectPool<Item>, IReadOnlyCollection<string>) CreatePool(
        PoolLoadMode loadMode,
        PoolStorageMode storageMode
    )
    {
        var nextId = 1u;
        var logs = new List<string>();

        void Log(string message)
        {
            lock (logs) logs.Add(message);
        }

        var sp = new ServiceContainer()
            .AddObjectPool<Item>(5, ServiceLifetime.Singleton, loadMode, storageMode)
            .Add(_ => new Item(nextId++, Log)).AsSelf().Transient()
            .BuildServiceProvider();

        var pool = sp.Resolve<IObjectPool<Item>>();

        return (pool, logs);
    }

    private static async Task Run(
        IObjectPool<Item> pool
    )
    {
        await Task.WhenAll(Enumerable.Range(0, Jobs).Select(i => Task.Run(() =>
        {
            var item = pool.Get();
            item.ExecuteAction(i);
            pool.Return(item);
        })));
    }

    private class Item : IDisposable
    {
        private readonly uint _id;
        private readonly Action<string> _log;

        public Item(
            uint id,
            Action<string> log
        )
        {
            _id = id;
            _log = log;
            log($"{id} {Created}");
        }

        public void ExecuteAction(int i)
        {
            Thread.Sleep(10);
            _log($"{_id} {Action} {i}");
        }

        public void Dispose()
        {
            _log($"{_id} {Disposed}");
        }
    }
}