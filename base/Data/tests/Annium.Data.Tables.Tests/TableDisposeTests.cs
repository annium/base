using System;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Data.Tables.Tests;

/// <summary>
/// Tests for table <c>DisposeAsync</c> behaviour: clean disposal on first use,
/// idempotent double-dispose, and table-cleared-on-dispose.
/// </summary>
public class TableDisposeTests : TestBase
{
    public TableDisposeTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(x => x.AddTables());
    }

    /// <summary>
    /// Disposing a table that was never used completes without throwing.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task DisposeAsync_NeverUsed_CompletesCleanly()
    {
        // arrange
        var table = Get<ITableFactory>()
            .New<Entry>()
            .Allow(TablePermission.All)
            .Key(x => x.Key)
            .Set((_, _) => true, (_, _) => { })
            .Build();

        // act + assert — simply awaiting must not throw
        await table.DisposeAsync();
    }

    /// <summary>
    /// Calling DisposeAsync a second time on the same instance completes without throwing
    /// (the IAsyncDisposable contract requires DisposeAsync to be idempotent).
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task DisposeAsync_Twice_DoesNotThrow()
    {
        // arrange
        var table = Get<ITableFactory>()
            .New<Entry>()
            .Allow(TablePermission.All)
            .Key(x => x.Key)
            .Set((_, _) => true, (_, _) => { })
            .Build();

        await table.DisposeAsync();

        // act + assert — second dispose is a no-op and must not throw
        await table.DisposeAsync();
    }

    /// <summary>
    /// After DisposeAsync completes, Count must be zero even when items were present before disposal.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task DisposeAsync_AfterData_ClearsTable()
    {
        // arrange — build a table and seed it with items
        var table = Get<ITableFactory>()
            .New<Entry>()
            .Allow(TablePermission.All)
            .Key(x => x.Key)
            .Set((a, b) => a.Value != b.Value, (s, v) => s.Update(v.Value))
            .Build();

        table.Init(new[] { new Entry(1, "a"), new Entry(2, "b"), new Entry(3, "c") });
        table.Count.Is(3);

        // act
        await table.DisposeAsync();

        // assert — internal dictionary must have been cleared
        table.Count.Is(0);
    }
}

/// <summary>
/// Test record representing a simple entry with a key and string value.
/// </summary>
/// <param name="Key">Unique identifier for the entry.</param>
/// <param name="Value">String payload of the entry.</param>
file record Entry(int Key, string Value) : ICopyable<Entry>
{
    /// <summary>
    /// Gets the string payload of the entry.
    /// </summary>
    public string Value { get; private set; } = Value;

    /// <summary>
    /// Updates the string payload of the entry.
    /// </summary>
    /// <param name="value">The new value.</param>
    public void Update(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a copy of the current entry.
    /// </summary>
    /// <returns>A new Entry instance that is a copy of the current instance.</returns>
    public Entry Copy() => this with { };
}
