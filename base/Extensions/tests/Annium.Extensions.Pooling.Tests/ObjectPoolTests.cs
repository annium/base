using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Descriptors;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Testing;
using Annium.Testing.Collection;
using Xunit;

namespace Annium.Extensions.Pooling.Tests;

/// <summary>
/// Tests for object pool functionality with different configurations
/// </summary>
public class ObjectPoolTests
{
    /// <summary>
    /// Maximum number of items in the pool
    /// </summary>
    private const int Capacity = 5;

    /// <summary>
    /// Number of concurrent jobs to execute during tests
    /// </summary>
    private const int Jobs = 20;

    /// <summary>
    /// Log message identifier for item creation events
    /// </summary>
    private const string Created = "Created";

    /// <summary>
    /// Log message identifier for action execution events
    /// </summary>
    private const string Action = "ExecuteAction";

    /// <summary>
    /// Log message identifier for item disposal events
    /// </summary>
    private const string Disposed = "Disposed";

    /// <summary>
    /// Tests that an eagerly loaded FIFO object pool correctly creates all items upfront,
    /// executes all jobs, and disposes all items
    /// </summary>
    /// <returns>Task representing the asynchronous test operation</returns>
    [Fact]
    public async Task ObjectPool_Eager_FIFO_Works()
    {
        // arrange
        var (pool, logs) = CreatePool(PoolLoadMode.Eager, PoolStorageMode.Fifo);

        // act
        await Run(pool);
        await pool.DisposeAsync();

        // assert
        // as eager - all workers Created
        logs.Where(x => x.Contains(Created)).Has(Capacity);
        // all job is done
        logs.Where(x => x.Contains(Action)).Has(Jobs);
        Enumerable.Range(0, Jobs).All(i => logs.Any(e => e.Contains($"{Action} {i}"))).IsTrue();
        // all workers are Disposed
        logs.Where(x => x.Contains(Disposed)).Has(Capacity);
    }

    /// <summary>
    /// Tests that a lazily loaded LIFO object pool correctly executes all jobs
    /// and disposes the same number of items as were created
    /// </summary>
    /// <returns>Task representing the asynchronous test operation</returns>
    [Fact]
    public async Task ObjectPool_Lazy_LIFO_Works()
    {
        // arrange
        var (pool, logs) = CreatePool(PoolLoadMode.Lazy, PoolStorageMode.Lifo);

        // act
        await Run(pool);
        await pool.DisposeAsync();

        // assert
        // all job is done
        Enumerable.Range(0, Jobs).All(i => logs.Any(e => e.Contains($"{Action} {i}"))).IsTrue();
        // all workers are Disposed
        (logs.Count(x => x.Contains(Disposed)) == logs.Count(x => x.Contains(Created))).IsTrue();
    }

    /// <summary>
    /// Creates an object pool with the specified configuration and returns it along with a log collection
    /// </summary>
    /// <param name="loadMode">The pool loading strategy (eager or lazy)</param>
    /// <param name="storageMode">The pool storage strategy (FIFO or LIFO)</param>
    /// <returns>A tuple containing the configured object pool and a collection for capturing log messages</returns>
    private (IObjectPool<Item>, IReadOnlyCollection<string>) CreatePool(
        PoolLoadMode loadMode,
        PoolStorageMode storageMode
    )
    {
        var nextId = 1u;
        var logs = new List<string>();

        void Log(string message)
        {
            lock (logs)
                logs.Add(message);
        }

        var sp = new ServiceContainer()
            .AddObjectPool<Item>(5, ServiceLifetime.Singleton, loadMode, storageMode)
            .Add(_ => new Item(nextId++, Log))
            .AsSelf()
            .Transient()
            .BuildServiceProvider();

        var pool = sp.Resolve<IObjectPool<Item>>();

        return (pool, logs);
    }

    /// <summary>
    /// Executes multiple concurrent jobs using the provided object pool
    /// </summary>
    /// <param name="pool">The object pool to use for getting and returning items</param>
    /// <returns>Task representing the asynchronous execution of all jobs</returns>
    private static async Task Run(IObjectPool<Item> pool)
    {
        await Task.WhenAll(
            Enumerable
                .Range(0, Jobs)
                .Select(i =>
                    Task.Run(() =>
                    {
                        var item = pool.Get();
                        item.ExecuteAction(i);
                        pool.Return(item);
                    })
                )
        );
    }

    /// <summary>
    /// Test item class that logs its lifecycle events and can execute actions
    /// </summary>
    private class Item : IDisposable
    {
        /// <summary>
        /// Unique identifier for this item instance
        /// </summary>
        private readonly uint _id;

        /// <summary>
        /// Logging action to record item lifecycle events
        /// </summary>
        private readonly Action<string> _log;

        /// <summary>
        /// Initializes a new item with the specified identifier and logging action
        /// </summary>
        /// <param name="id">Unique identifier for this item</param>
        /// <param name="log">Action to call for logging events</param>
        public Item(uint id, Action<string> log)
        {
            _id = id;
            _log = log;
            log($"{id} {Created}");
        }

        /// <summary>
        /// Executes a test action with the specified job identifier
        /// </summary>
        /// <param name="i">Job identifier to include in log messages</param>
        public void ExecuteAction(int i)
        {
            Thread.Sleep(10);
            _log($"{_id} {Action} {i}");
        }

        /// <summary>
        /// Disposes the item and logs the disposal event
        /// </summary>
        public void Dispose()
        {
            _log($"{_id} {Disposed}");
        }
    }
}
