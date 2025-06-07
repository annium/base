using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Testing;
using Xunit;

namespace Annium.Data.Tables.Tests;

/// <summary>
/// Tests for basic table functionality including subscription and initialization.
/// </summary>
public class TableTests : TestBase
{
    public TableTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(x => x.AddTables());
    }

    /// <summary>
    /// Tests that table subscription and initialization work correctly with proper event emission.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Works()
    {
        // arrange
        var table = Get<ITableFactory>()
            .New<Sample>()
            .Allow(TablePermission.All)
            .Key(x => x.Key)
            .Keep(x => x.IsAlive)
            .Set((a, b) => a.IsAlive != b.IsAlive, (s, v) => s.Update(v.IsAlive))
            .Build();
        var log1 = new List<ChangeEvent<Sample>>();
        var log2 = new List<ChangeEvent<Sample>>();

        // subscribe will emit init events
        table.Subscribe(log1.Add);
        table.Subscribe(log2.Add);
        log1.Has(1);
        log1.At(0).Is(ChangeEvent.Init(Array.Empty<Sample>()));
        log2.At(0).Is(log1.At(0));

        // init with some data
        var initValues = new[] { new Sample(1, true) };
        table.Init(initValues);
        await Expect.ToAsync(() => log1.Count.IsGreater(1));
        log1.Has(2);
        log1.At(1).Equals(ChangeEvent.Init(initValues)).IsTrue();
        log2.At(1).Is(log1.At(1));
    }
}

/// <summary>
/// Test record representing a sample with a key and alive status, implementing ICopyable for table operations.
/// </summary>
/// <param name="Key">The unique identifier for the sample.</param>
/// <param name="IsAlive">The alive status of the sample.</param>
file record Sample(int Key, bool IsAlive) : ICopyable<Sample>
{
    /// <summary>
    /// Gets the alive status of the sample.
    /// </summary>
    public bool IsAlive { get; private set; } = IsAlive;

    /// <summary>
    /// Updates the alive status of the sample.
    /// </summary>
    /// <param name="isAlive">The new alive status value.</param>
    public void Update(bool isAlive)
    {
        IsAlive = isAlive;
    }

    /// <summary>
    /// Creates a copy of the current sample.
    /// </summary>
    /// <returns>A new Sample instance that is a copy of the current instance.</returns>
    public Sample Copy() => this with { };
}
