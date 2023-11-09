using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
            container.AddObjectCache<uint, Item, ItemProvider>(ServiceLifetime.Singleton);
            container.Add<Log>().AsSelf().Singleton();
        });
    }

    [Fact]
    public async Task ObjectCache_Create_Works()
    {
        this.Trace("start");

        // arrange
        await using var cache = Get<IObjectCache<uint, Item>>();
        var log = Get<Log>();

        // act
        this.Trace("get multiple references");
        var references = await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => cache.GetAsync(0)));

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
        var cache = Get<IObjectCache<uint, Item>>();
        var log = Get<Log>();

        // act
        await Task.WhenAll(
            Enumerable
                .Range(0, 30)
                .Select(async i =>
                {
                    await using var reference = await cache.GetAsync((uint)i % 2);
                    await Task.Delay(20);
                })
        );
        var references = await Task.WhenAll(Enumerable.Range(0, 20).Select(i => cache.GetAsync((uint)i % 2)));
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

    private class ItemProvider : ObjectCacheProvider<uint, Item>
    {
        private readonly Log _log;

        public ItemProvider(Log log)
        {
            _log = log;
        }

        public override async Task<OneOf<Item, IDisposableReference<Item>>> CreateAsync(uint id, CancellationToken ct)
        {
            await Task.Delay(10);

            var item = new Item(id);
            _log.Add($"{item} {Created}");

            return item;
        }

        public override async Task SuspendAsync(uint id, Item value)
        {
            await value.Suspend();
            _log.Add($"{value} {Suspended}");
        }

        public override async Task ResumeAsync(uint id, Item value)
        {
            await value.Resume();
            _log.Add($"{value} {Resumed}");
        }

        public override Task DisposeAsync(uint id, Item value)
        {
            value.Dispose();
            _log.Add($"{value} {Disposed}");

            return Task.CompletedTask;
        }
    }

    private class Item : IDisposable
    {
        private readonly uint _id;

        public Item(uint id)
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

    private class Log : IReadOnlyCollection<string>
    {
        private readonly ConcurrentQueue<string> _entries = new();

        public int Count => _entries.Count;

        public void Add(string message) => _entries.Enqueue(message);

        public IEnumerator<string> GetEnumerator() => _entries.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _entries.GetEnumerator();
    }
}
