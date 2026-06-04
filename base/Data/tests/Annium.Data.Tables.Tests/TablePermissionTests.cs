using System;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Data.Tables.Tests;

/// <summary>
/// Tests that verify permission enforcement on table operations and the inactive-item auto-delete behaviour.
/// </summary>
public class TablePermissionTests : TestBase
{
    public TablePermissionTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(x => x.AddTables());
    }

    // ── TG-A: EnsurePermission throws when the required flag is absent ────────

    /// <summary>
    /// Calling Init on a table that was not granted the Init permission throws
    /// InvalidOperationException.
    /// </summary>
    [Fact]
    public void Init_WithoutInitPermission_Throws()
    {
        // arrange — Add only, Init is intentionally absent
        var table = Get<ITableFactory>().New<Item>().Allow(TablePermission.Add).Key(x => x.Key).Build();

        // act + assert
        Wrap.It(() => table.Init(new[] { new Item(1, true) })).Throws<InvalidOperationException>();
    }

    /// <summary>
    /// Calling Set with a key that does not yet exist (Add path) on a table that has no Add
    /// permission throws InvalidOperationException.
    /// </summary>
    [Fact]
    public void Set_WithoutAddPermission_Throws()
    {
        // arrange — Init|Delete only, Add is absent; Update is also absent so no update delegate needed
        var table = Get<ITableFactory>()
            .New<Item>()
            .Allow(TablePermission.Init | TablePermission.Delete)
            .Key(x => x.Key)
            .Build();

        // act + assert — key 1 has never been inserted, so Set takes the Add branch
        Wrap.It(() => table.Set(new Item(1, true))).Throws<InvalidOperationException>();
    }

    /// <summary>
    /// Calling Set on an already-existing key (Update path) on a table that has no Update
    /// permission throws InvalidOperationException.
    /// </summary>
    [Fact]
    public void Set_WithoutUpdatePermission_Throws()
    {
        // arrange — Init|Add only, Update is absent
        // We do NOT include Update so we must NOT call .Set(hasChanged, update) on the builder,
        // but we still need to seed the key via Init; the default hasChanged always returns true.
        var table = Get<ITableFactory>()
            .New<Item>()
            .Allow(TablePermission.Init | TablePermission.Add)
            .Key(x => x.Key)
            .Build();

        // seed so the key exists — Init is permitted
        table.Init(new[] { new Item(1, true) });

        // act + assert — key 1 exists now, so Set takes the Update branch
        Wrap.It(() => table.Set(new Item(1, false))).Throws<InvalidOperationException>();
    }

    /// <summary>
    /// Calling Delete on a table that has no Delete permission throws
    /// InvalidOperationException.
    /// </summary>
    [Fact]
    public void Delete_WithoutDeletePermission_Throws()
    {
        // arrange — Init|Add|Update only, Delete is absent
        var table = Get<ITableFactory>()
            .New<Item>()
            .Allow(TablePermission.Init | TablePermission.Add | TablePermission.Update)
            .Key(x => x.Key)
            .Set((a, b) => a.IsAlive != b.IsAlive, (s, v) => s.Update(v.IsAlive))
            .Build();

        // act + assert
        Wrap.It(() => table.Delete(new Item(1, true))).Throws<InvalidOperationException>();
    }

    // ── TG-D: Set updating an item to inactive auto-deletes it ───────────────

    /// <summary>
    /// When Set is called with a value that makes the existing item inactive (Keep predicate
    /// returns false after the update), the table must emit a Delete ChangeEvent and the Count
    /// must drop to zero.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task Set_UpdateToInactive_EmitsDeleteAndDropsCount()
    {
        // arrange
        var table = Get<ITableFactory>()
            .New<Item>()
            .Allow(TablePermission.All)
            .Key(x => x.Key)
            .Keep(x => x.IsAlive)
            .Set((a, b) => a.IsAlive != b.IsAlive, (s, v) => s.Update(v.IsAlive))
            .Build();

        // seed one active item — Set takes the Add branch, so Add is exercised
        table.Set(new Item(1, true));
        table.Count.Is(1);

        var log = new TestLog<ChangeEvent<Item>>();
        table.Subscribe(log.Add);

        // wait for the initial snapshot event to be delivered
        await Expect.ToAsync(() => log.Has(1));
        log.At(0).Type.Is(ChangeEventType.Init);

        // act — updating IsAlive to false makes the item inactive;
        // CleanupOutsideLock must remove it and emit a Delete event
        table.Set(new Item(1, false));

        // assert — pipeline must deliver the Delete event
        await Expect.ToAsync(() => log.Has(2));
        log.At(1).Type.Is(ChangeEventType.Delete);
        log.At(1).Item.Key.Is(1);
        table.Count.Is(0);
    }
}

/// <summary>
/// Test record representing an item with a key and alive status.
/// </summary>
/// <param name="Key">Unique identifier for the item.</param>
/// <param name="IsAlive">Whether the item is considered active.</param>
file record Item(int Key, bool IsAlive) : ICopyable<Item>
{
    /// <summary>
    /// Gets or sets the alive status of the item.
    /// </summary>
    public bool IsAlive { get; private set; } = IsAlive;

    /// <summary>
    /// Updates the alive status of the item.
    /// </summary>
    /// <param name="isAlive">The new alive status value.</param>
    public void Update(bool isAlive)
    {
        IsAlive = isAlive;
    }

    /// <summary>
    /// Creates a copy of the current item.
    /// </summary>
    /// <returns>A new Item instance that is a copy of the current instance.</returns>
    public Item Copy() => this with { };
}
