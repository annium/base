using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Testing;
using Annium.Testing.Lib;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Data.Tables.Tests;

public class TableSourceExtensionsTests : TestBase
{
    public TableSourceExtensionsTests(ITestOutputHelper outputHelper) : base(outputHelper)
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
            .Build();
        source.Init(new[] { new Raw(1, 2), new Raw(2, 3) });
        var target = Get<ITableFactory>()
            .New<Sample>()
            .Allow(TablePermission.All)
            .Key(x => x.Key)
            .Build();
        var log = new List<IChangeEvent<Sample>>();

        // arrange - prepare log
        target.Subscribe(log.Add);
        log.Has(1);
        log.At(0).As<InitEvent<Sample>>().Values.IsEqual(Array.Empty<Sample>());

        // setup map
        source.MapWriteTo(target, x => new Sample(x.Key, x.Stamp.ToString()));
        source.Init(new[] { new Raw(1, 3), new Raw(2, 4) });
        source.Set(new Raw(3, 5));
        source.Set(new Raw(3, 6));
        source.Delete(new Raw(3, 0));

        // assert
        log.Has(6);
        // appears when map is set up
        log.At(1).As<InitEvent<Sample>>().Values.IsEqual(new[] { new Sample(1, "2"), new Sample(2, "3") });
        // appears when init is run on source
        log.At(2).As<InitEvent<Sample>>().Values.IsEqual(new[] { new Sample(1, "3"), new Sample(2, "4") });
        log.At(3).As<AddEvent<Sample>>().Value.IsEqual(new Sample(3, "6")); // 6, not 5, because added entity is passed by reference
        log.At(4).As<UpdateEvent<Sample>>().Value.IsEqual(new Sample(3, "6"));
        log.At(5).As<DeleteEvent<Sample>>().Value.IsEqual(new Sample(3, "6"));
    }

    [Fact]
    public void MapAppendTo_Works()
    {
        // arrange - init
        var source = Get<ITableFactory>()
            .New<Raw>()
            .Allow(TablePermission.All)
            .Key(x => x.Key)
            .Build();
        source.Init(new[] { new Raw(1, 2), new Raw(2, 3) });
        var target = Get<ITableFactory>()
            .New<Sample>()
            .Allow(TablePermission.All)
            .Key(x => x.Key)
            .Build();
        var log = new List<IChangeEvent<Sample>>();

        // arrange - prepare log
        target.Subscribe(log.Add);
        log.Has(1);
        log.At(0).As<InitEvent<Sample>>().Values.IsEqual(Array.Empty<Sample>());

        // setup map
        source.MapAppendTo(target, x => new Sample(x.Key, x.Stamp.ToString()));
        source.Init(new[] { new Raw(1, 3), new Raw(2, 4) });
        source.Set(new Raw(3, 5));
        source.Set(new Raw(3, 6));
        source.Delete(new Raw(3, 0));

        // assert
        log.Has(8);
        // appears when map is set up
        log.At(1).As<AddEvent<Sample>>().Value.IsEqual(new Sample(1, "3")); // 3, not 2, because added entity is passed by reference
        log.At(2).As<AddEvent<Sample>>().Value.IsEqual(new Sample(2, "4")); // 4, not 3, because added entity is passed by reference
        // appears when init is run on source
        log.At(3).As<UpdateEvent<Sample>>().Value.IsEqual(new Sample(1, "3"));
        log.At(4).As<UpdateEvent<Sample>>().Value.IsEqual(new Sample(2, "4"));
        log.At(5).As<AddEvent<Sample>>().Value.IsEqual(new Sample(3, "6")); // 6, not 5, because added entity is passed by reference
        log.At(6).As<UpdateEvent<Sample>>().Value.IsEqual(new Sample(3, "6"));
        log.At(7).As<DeleteEvent<Sample>>().Value.IsEqual(new Sample(3, "6"));
    }

    [Fact]
    public async Task SyncAddRemove_Works()
    {
        // arrange
        var table = Get<ITableFactory>()
            .New<Sample>()
            .Allow(TablePermission.All)
            .Key(x => x.Key)
            .Build();
        var initValues = new[] { new Sample(1, "a"), new Sample(2, "b") };
        table.Init(initValues);
        var log = new List<IChangeEvent<Sample>>();

        // subscribe will emit init events
        table.Subscribe(log.Add);
        log.Has(1);
        log.At(0).IsEqual(ChangeEvent.Init(initValues));

        // sync with some data
        var syncValues = new[] { new Sample(1, "a"), new Sample(3, "c") };
        table.SyncAddDelete(syncValues);
        await Task.Delay(100);
        log.Has(3);
        log.At(1).IsEqual(ChangeEvent.Delete(initValues[1]));
        log.At(2).IsEqual(ChangeEvent.Add(syncValues[1]));
    }

    [Fact]
    public async Task SyncAddUpdateRemove_Works()
    {
        // arrange
        var table = Get<ITableFactory>()
            .New<Sample>()
            .Allow(TablePermission.All)
            .Key(x => x.Key)
            .Build();
        var initValues = new[] { new Sample(1, "a"), new Sample(2, "b") };
        table.Init(initValues);
        var log = new List<IChangeEvent<Sample>>();

        // subscribe will emit init events
        table.Subscribe(log.Add);
        log.Has(1);
        log.At(0).IsEqual(ChangeEvent.Init(initValues));

        // sync with some data
        var syncValues = new[] { new Sample(1, "z"), new Sample(3, "c") };
        table.SyncAddUpdateDelete(syncValues);
        await Task.Delay(100);
        log.Has(4);
        log.At(1).IsEqual(ChangeEvent.Delete(initValues[1]));
        log.At(2).IsEqual(ChangeEvent.Update(syncValues[0]));
        log.At(3).IsEqual(ChangeEvent.Add(syncValues[1]));
    }
}

file sealed record Sample(int Key, string Data) : ICopyable<Sample>
{
    public Sample Copy() => this with { };
}

file sealed record Raw(int Key, long Stamp) : ICopyable<Raw>
{
    public Raw Copy() => this with { };
}