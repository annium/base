using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Data.Tables.Tests;

/// <summary>
/// Tests for table source extension methods including mapping and synchronization operations.
/// </summary>
public class TableSourceExtensionsTests : TestBase
{
    public TableSourceExtensionsTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(x => x.AddTables());
    }

    /// <summary>
    /// Tests that MapWriteTo correctly maps and writes data from source table to target table with full replacement.
    /// </summary>
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

    /// <summary>
    /// Tests that MapAppendTo correctly maps and appends data from source table to target table without replacement.
    /// </summary>
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

    /// <summary>
    /// Tests that SyncAddDelete correctly synchronizes tables by adding new items and removing obsolete ones.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
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
        await Task.Delay(100, TestContext.Current.CancellationToken);
        log.Has(3);
        log.At(1).Equals(ChangeEvent.Delete(initValues[1])).IsTrue();
        log.At(2).Equals(ChangeEvent.Set(syncValues[1])).IsTrue();
    }

    /// <summary>
    /// Tests that SyncAddUpdateDelete correctly synchronizes tables by adding, updating, and removing items as needed.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
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
        await Task.Delay(100, TestContext.Current.CancellationToken);
        log.Has(4);
        log.At(1).Equals(ChangeEvent.Delete(initValues[1])).IsTrue();
        log.At(2).Equals(ChangeEvent.Set(syncValues[0])).IsTrue();
        log.At(3).Equals(ChangeEvent.Set(syncValues[1])).IsTrue();
    }
}

/// <summary>
/// Test record representing a sample with a key and data, implementing ICopyable for table operations.
/// </summary>
/// <param name="Key">The unique identifier for the sample.</param>
/// <param name="Data">The data content of the sample.</param>
file sealed record Sample(int Key, string Data) : ICopyable<Sample>
{
    /// <summary>
    /// Gets the data content of the sample.
    /// </summary>
    public string Data { get; private set; } = Data;

    /// <summary>
    /// Updates the data content of the sample.
    /// </summary>
    /// <param name="data">The new data value.</param>
    public void Update(string data)
    {
        Data = data;
    }

    /// <summary>
    /// Creates a copy of the current sample.
    /// </summary>
    /// <returns>A new Sample instance that is a copy of the current instance.</returns>
    public Sample Copy() => this with { };
}

/// <summary>
/// Test record representing raw data with a key and timestamp, implementing ICopyable for table operations.
/// </summary>
/// <param name="Key">The unique identifier for the raw data.</param>
/// <param name="Stamp">The timestamp value for the raw data.</param>
file sealed record Raw(int Key, long Stamp) : ICopyable<Raw>
{
    /// <summary>
    /// Gets the timestamp value of the raw data.
    /// </summary>
    public long Stamp { get; private set; } = Stamp;

    /// <summary>
    /// Updates the timestamp value of the raw data.
    /// </summary>
    /// <param name="stamp">The new timestamp value.</param>
    public void Update(long stamp)
    {
        Stamp = stamp;
    }

    /// <summary>
    /// Creates a copy of the current raw data.
    /// </summary>
    /// <returns>A new Raw instance that is a copy of the current instance.</returns>
    public Raw Copy() => this with { };
}
