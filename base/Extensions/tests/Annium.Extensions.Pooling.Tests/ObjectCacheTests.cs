using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Testing;
using OneOf;
using Xunit;

namespace Annium.Extensions.Pooling.Tests;

/// <summary>
/// Test class for object cache functionality
/// </summary>
public class ObjectCacheTests : TestBase
{
    /// <summary>
    /// Constant for created state
    /// </summary>
    private const string Created = "Created";

    /// <summary>
    /// Constant for suspended state
    /// </summary>
    private const string Suspended = "Suspended";

    /// <summary>
    /// Constant for resumed state
    /// </summary>
    private const string Resumed = "Resumed";

    /// <summary>
    /// Constant for disposed state
    /// </summary>
    private const string Disposed = "Disposed";

    public ObjectCacheTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container =>
        {
            container.AddObjectCache<ItemKey, Item, ItemProvider>(ServiceLifetime.Singleton);
        });
        this.RegisterTestLogs();
    }

    /// <summary>
    /// Tests that object cache creation works correctly
    /// </summary>
    /// <returns>Task representing the asynchronous test operation</returns>
    [Fact]
    public async Task ObjectCache_Create_Works()
    {
        this.Trace("start");

        // arrange
        await using var cache = Get<IObjectCache<ItemKey, Item>>();
        var log = Get<TestLog<string>>();

        // act
        this.Trace("get multiple references");
        var references = await Task.WhenAll(
            Enumerable.Range(0, 10).Select(_ => Task.Run(() => cache.GetAsync(new ItemKey { Value = 0 })))
        );

        // assert
        this.Trace("assert references validity");
        references.Has(10);
        references.All(x => x.Value != default!).IsTrue();
        references.GroupBy(x => x.Value).Has(1);
        log.Has(1);
        log.ElementAt(0).Is($"0 {Created}");

        this.Trace("assert references validity");
    }

    /// <summary>
    /// Tests that object cache suspension works correctly
    /// </summary>
    /// <returns>Task representing the asynchronous test operation</returns>
    [Fact]
    public async Task ObjectCache_Suspend_Works()
    {
        // arrange
        var cache = Get<IObjectCache<ItemKey, Item>>();
        var log = Get<TestLog<string>>();

        // act
        await Task.WhenAll(
            Enumerable
                .Range(0, 30)
                .Select(async i =>
                {
                    await using var reference = await cache.GetAsync(new ItemKey { Value = i % 2 });
                    await Task.Delay(20);
                })
        );
        var references = await Task.WhenAll(
            Enumerable.Range(0, 20).Select(i => Task.Run(() => cache.GetAsync(new ItemKey { Value = i % 2 })))
        );
        await cache.DisposeAsync();

        // assert
        references.Has(20);
        references.GroupBy(x => x.Value).Has(2);
        log.Count.IsNotDefault();
        Enumerable.Range(0, 2).Select(x => $"{x} {Created}").All(log.Contains).IsTrue();
        foreach (var i in Enumerable.Range(0, 2))
        {
            log.Contains($"{i} {Suspended}").IsTrue();
            log.Contains($"{i} {Resumed}").IsTrue();
        }

        Enumerable.Range(0, 2).Select(x => $"{x} {Disposed}").All(log.Contains).IsTrue();
    }

    /// <summary>
    /// Test item provider for object cache
    /// </summary>
    private class ItemProvider : ObjectCacheProvider<ItemKey, Item>
    {
        /// <summary>
        /// Test logging instance
        /// </summary>
        private readonly TestLog<string> _log;

        public ItemProvider(TestLog<string> log)
        {
            _log = log;
        }

        /// <summary>
        /// Creates a new item asynchronously
        /// </summary>
        /// <param name="id">The item key</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Task containing the created item or disposable reference</returns>
        public override async Task<OneOf<Item, IDisposableReference<Item>>> CreateAsync(
            ItemKey id,
            CancellationToken ct
        )
        {
            await Task.Delay(10);

            var item = new Item(id);
            _log.Add($"{item} {Created}");

            return item;
        }

        /// <summary>
        /// Suspends an item asynchronously
        /// </summary>
        /// <param name="id">The item key</param>
        /// <param name="value">The item to suspend</param>
        /// <returns>Task representing the asynchronous suspension operation</returns>
        public override async Task SuspendAsync(ItemKey id, Item value)
        {
            await value.Suspend();
            _log.Add($"{value} {Suspended}");
        }

        /// <summary>
        /// Resumes an item asynchronously
        /// </summary>
        /// <param name="id">The item key</param>
        /// <param name="value">The item to resume</param>
        /// <returns>Task representing the asynchronous resume operation</returns>
        public override async Task ResumeAsync(ItemKey id, Item value)
        {
            await value.Resume();
            _log.Add($"{value} {Resumed}");
        }

        /// <summary>
        /// Disposes an item asynchronously
        /// </summary>
        /// <param name="id">The item key</param>
        /// <param name="value">The item to dispose</param>
        /// <returns>Task representing the asynchronous disposal operation</returns>
        public override Task DisposeAsync(ItemKey id, Item value)
        {
            value.Dispose();
            _log.Add($"{value} {Disposed}");

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Test item class for object cache testing
    /// </summary>
    private class Item : IDisposable
    {
        /// <summary>
        /// The item's unique identifier
        /// </summary>
        private readonly ItemKey _id;

        public Item(ItemKey id)
        {
            _id = id;
        }

        /// <summary>
        /// Suspends the item
        /// </summary>
        /// <returns>Task representing the asynchronous suspension operation</returns>
        public async Task Suspend()
        {
            await Task.Delay(10);
        }

        /// <summary>
        /// Resumes the item
        /// </summary>
        /// <returns>Task representing the asynchronous resume operation</returns>
        public async Task Resume()
        {
            await Task.Delay(10);
        }

        /// <summary>
        /// Disposes the item
        /// </summary>
        public void Dispose() { }

        /// <summary>
        /// Returns a string representation of the item
        /// </summary>
        /// <returns>String representation of the item</returns>
        public override string ToString() => _id.ToString();
    }
}

/// <summary>
/// Test key record for identifying cached items
/// </summary>
public sealed record ItemKey
{
    /// <summary>
    /// Gets the key value
    /// </summary>
    public required int Value { get; init; }

    /// <summary>
    /// Returns a string representation of the key
    /// </summary>
    /// <returns>String representation of the key value</returns>
    public override string ToString() => Value.ToString();
}
