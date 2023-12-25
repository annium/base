using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Data.Tables.Tests;

public class TableSourceExtensionsTests : TestBase
{
    public TableSourceExtensionsTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(x => x.AddTables());
    }

    [Fact]
    public void MapWriteTo_Works()
    {
        // arrange - init
        var source = Get<ITableFactory>()
            .New<Raw>()
            .Allow(TablePermission.All)
            .Key(x => x.Key)
            .Set((a, b) => a.Stamp != b.Stamp, (s, v) => s.Update(v.Stamp))
            .Build();
        source.Init(new[] { new Raw(1, 2), new Raw(2, 3) });
        var target = Get<ITableFactory>()
            .New<Sample>()
            .Allow(TablePermission.All)
            .Key(x => x.Key)
            .Set((a, b) => a.Data != b.Data, (s, v) => s.Update(v.Data))
            .Build();
        var log = new TestLog<ChangeEvent<Sample>>();

        // arrange - prepare log
        target.Subscribe(log.Add);
        log.Has(1);
        log.At(0).Items.Is(Array.Empty<Sample>());

        // setup map
        source.MapWriteTo(target, x => new Sample(x.Key, x.Stamp.ToString()));
        source.Init(new[] { new Raw(1, 3), new Raw(2, 4) });
        source.Set(new Raw(3, 5));
        source.Set(new Raw(3, 6));
        source.Delete(new Raw(3, 0));

        // assert
        log.Has(6);
        // appears when map is set up
        log.At(1).Items.IsEqual(new[] { new Sample(1, "2"), new Sample(2, "3") });
        // appears when init is run on source
        log.At(2).Items.IsEqual(new[] { new Sample(1, "3"), new Sample(2, "4") });
        log.At(3).Item.Is(new Sample(3, "6")); // 6, not 5, because added entity is passed by reference
        log.At(4).Item.Is(new Sample(3, "6"));
        log.At(5).Item.Is(new Sample(3, "6"));
    }

    [Fact]
    public void MapAppendTo_Works()
    {
        // arrange - init
        var source = Get<ITableFactory>()
            .New<Raw>()
            .Allow(TablePermission.All)
            .Key(x => x.Key)
            .Set((a, b) => a.Stamp != b.Stamp, (s, v) => s.Update(v.Stamp))
            .Build();
        source.Init(new[] { new Raw(1, 2), new Raw(2, 3) });
        var target = Get<ITableFactory>()
            .New<Sample>()
            .Allow(TablePermission.All)
            .Key(x => x.Key)
            .Set((a, b) => a.Data != b.Data, (s, v) => s.Update(v.Data))
            .Build();
        var log = new List<ChangeEvent<Sample>>();

        // arrange - prepare log
        target.Subscribe(x => log.Add(x));
        log.Has(1);
        log.At(0).Items.IsEqual(Array.Empty<Sample>());

        // setup map
        source.MapAppendTo(target, x => new Sample(x.Key, x.Stamp.ToString()));
        source.Init(new[] { new Raw(1, 3), new Raw(2, 4) });
        source.Set(new Raw(3, 5));
        source.Set(new Raw(3, 6));
        source.Delete(new Raw(3, 0));

        // assert
        log.Has(8);
        // appears when map is set up
        log.At(1).Item.IsEqual(new Sample(1, "3")); // 3, not 2, because added entity is passed by reference
        log.At(2).Item.IsEqual(new Sample(2, "4")); // 4, not 3, because added entity is passed by reference
        // appears when init is run on source
        log.At(3).Item.IsEqual(new Sample(1, "3"));
        log.At(4).Item.IsEqual(new Sample(2, "4"));
        log.At(5).Item.IsEqual(new Sample(3, "6")); // 6, not 5, because added entity is passed by reference
        log.At(6).Item.IsEqual(new Sample(3, "6"));
        log.At(7).Item.IsEqual(new Sample(3, "6"));
    }

    [Fact]
    public async Task SyncAddRemove_Works()
    {
        // arrange
        var table = Get<ITableFactory>()
            .New<Sample>()
            .Allow(TablePermission.All)
            .Key(x => x.Key)
            .Set((a, b) => a.Data != b.Data, (s, v) => s.Update(v.Data))
            .Build();
        var initValues = new[] { new Sample(1, "a"), new Sample(2, "b") };
        table.Init(initValues);
        var log = new List<ChangeEvent<Sample>>();

        // subscribe will emit init events
        table.Subscribe(log.Add);
        log.Has(1);
        log.At(0).Equals(ChangeEvent.Init(initValues)).IsTrue();

        // sync with some data
        var syncValues = new[] { new Sample(1, "a"), new Sample(3, "c") };
        table.SyncAddDelete(syncValues);
        await Task.Delay(100);
        log.Has(3);
        log.At(1).Equals(ChangeEvent.Delete(initValues[1])).IsTrue();
        log.At(2).Equals(ChangeEvent.Set(syncValues[1])).IsTrue();
    }

    [Fact]
    public async Task SyncAddUpdateRemove_Works()
    {
        // arrange
        var table = Get<ITableFactory>()
            .New<Sample>()
            .Allow(TablePermission.All)
            .Key(x => x.Key)
            .Set((a, b) => a.Data != b.Data, (s, v) => s.Update(v.Data))
            .Build();
        var initValues = new[] { new Sample(1, "a"), new Sample(2, "b") };
        table.Init(initValues);
        var log = new List<ChangeEvent<Sample>>();

        // subscribe will emit init events
        table.Subscribe(log.Add);
        log.Has(1);
        log.At(0).Equals(ChangeEvent.Init(initValues)).IsTrue();

        // sync with some data
        var syncValues = new[] { new Sample(1, "z"), new Sample(3, "c") };
        table.SyncAddUpdateDelete(syncValues);
        await Task.Delay(100);
        log.Has(4);
        log.At(1).Equals(ChangeEvent.Delete(initValues[1])).IsTrue();
        log.At(2).Equals(ChangeEvent.Set(syncValues[0])).IsTrue();
        log.At(3).Equals(ChangeEvent.Set(syncValues[1])).IsTrue();
    }
}

file sealed record Sample(int Key, string Data) : ICopyable<Sample>
{
    public string Data { get; private set; } = Data;

    public void Update(string data)
    {
        Data = data;
    }

    public Sample Copy() => this with { };
}

file sealed record Raw(int Key, long Stamp) : ICopyable<Raw>
{
    public long Stamp { get; private set; } = Stamp;

    public void Update(long stamp)
    {
        Stamp = stamp;
    }

    public Raw Copy() => this with { };
}
