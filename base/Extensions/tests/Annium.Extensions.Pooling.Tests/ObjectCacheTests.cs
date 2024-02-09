using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Testing;
using OneOf;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Extensions.Pooling.Tests;

public class ObjectCacheTests : TestBase
{
    private const string Created = "Created";
    private const string Suspended = "Suspended";
    private const string Resumed = "Resumed";
    private const string Disposed = "Disposed";

    public ObjectCacheTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container =>
        {
            container.AddObjectCache<ItemKey, Item, ItemProvider>(ServiceLifetime.Singleton);
        });
        RegisterTestLogs();
    }

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

    private class ItemProvider : ObjectCacheProvider<ItemKey, Item>
    {
        private readonly TestLog<string> _log;

        public ItemProvider(TestLog<string> log)
        {
            _log = log;
        }

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

        public override async Task SuspendAsync(ItemKey id, Item value)
        {
            await value.Suspend();
            _log.Add($"{value} {Suspended}");
        }

        public override async Task ResumeAsync(ItemKey id, Item value)
        {
            await value.Resume();
            _log.Add($"{value} {Resumed}");
        }

        public override Task DisposeAsync(ItemKey id, Item value)
        {
            value.Dispose();
            _log.Add($"{value} {Disposed}");

            return Task.CompletedTask;
        }
    }

    private class Item : IDisposable
    {
        private readonly ItemKey _id;

        public Item(ItemKey id)
        {
            _id = id;
        }

        public async Task Suspend()
        {
            await Task.Delay(10);
        }

        public async Task Resume()
        {
            await Task.Delay(10);
        }

        public void Dispose() { }

        public override string ToString() => _id.ToString();
    }
}

public sealed record ItemKey
{
    public required int Value { get; init; }

    public override string ToString() => Value.ToString();
}
