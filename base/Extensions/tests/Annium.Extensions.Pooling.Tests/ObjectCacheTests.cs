using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Testing;
using OneOf;
using Xunit;

namespace Annium.Extensions.Pooling.Tests;

public class ObjectCacheTests
{
    private const string Created = "Created";
    private const string Suspended = "Suspended";
    private const string Resumed = "Resumed";
    private const string Disposed = "Disposed";

    [Fact]
    public async Task ObjectCache_Create_Works()
    {
        // arrange
        var (cache, logs) = CreateCache();

        // act
        var references = await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => cache.GetAsync(0)));

        // assert
        references.Has(10);
        references.All(x => x.Value != default!).IsTrue();
        references.GroupBy(x => x.Value).Has(1);
        logs.Has(1);
        logs.ElementAt(0).Is($"0 {Created}");
    }

    [Fact]
    public async Task ObjectCache_Suspend_Works()
    {
        // arrange
        var (cache, logs) = CreateCache();

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
        logs.Count.IsNotDefault();
        Enumerable.Range(0, 2).Select(x => $"{x} {Created}").All(logs.Contains).IsTrue();
        foreach (var i in Enumerable.Range(0, 2))
        {
            logs.Contains($"{i} {Suspended}").IsTrue();
            logs.Contains($"{i} {Resumed}").IsTrue();
        }

        Enumerable.Range(0, 2).Select(x => $"{x} {Disposed}").All(logs.Contains).IsTrue();
    }

    private (IObjectCache<uint, Item>, IReadOnlyCollection<string>) CreateCache()
    {
        var logs = new List<string>();

        void Log(string message)
        {
            lock (logs)
                logs.Add(message);
        }

        var sp = new ServiceContainer()
            .AddTime()
            .WithManagedTime()
            .SetDefault()
            .AddLogging()
            .AddObjectCache<uint, Item, ItemProvider>(ServiceLifetime.Singleton)
            .Add<Action<string>>(Log)
            .AsSelf()
            .Singleton()
            .BuildServiceProvider()
            .UseLogging(route => route.UseInMemory());
        var cache = sp.Resolve<IObjectCache<uint, Item>>();

        return (cache, logs);
    }

    private class ItemProvider : ObjectCacheProvider<uint, Item>
    {
        private readonly Action<string> _log;

        public ItemProvider(Action<string> log)
        {
            _log = log;
        }

        public override async Task<OneOf<Item, IDisposableReference<Item>>> CreateAsync(uint id, CancellationToken ct)
        {
            await Task.Delay(10);

            var item = new Item(id);
            _log($"{item} {Created}");

            return item;
        }

        public override async Task SuspendAsync(Item value)
        {
            await value.Suspend();
            _log($"{value} {Suspended}");
        }

        public override async Task ResumeAsync(Item value)
        {
            await value.Resume();
            _log($"{value} {Resumed}");
        }

        public override Task DisposeAsync(Item value)
        {
            value.Dispose();
            _log($"{value} {Disposed}");

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
}
