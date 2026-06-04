using System;
using Annium.Testing;
using Xunit;

namespace Annium.Data.Tables.Tests;

/// <summary>
/// Tests for <see cref="ChangeEvent{T}"/> guard properties: verifies that accessing
/// <c>Item</c> on an Init event and <c>Items</c> on Set/Delete events throw
/// <see cref="InvalidOperationException"/>, and that the happy paths return the expected values.
/// </summary>
public class ChangeEventTests
{
    // ── TG-E: guard / type checks ─────────────────────────────────────────────

    /// <summary>
    /// Accessing Item on an Init event throws InvalidOperationException.
    /// </summary>
    [Fact]
    public void Item_OnInitEvent_Throws()
    {
        // arrange
        var evt = ChangeEvent.Init(new[] { 1, 2, 3 });

        // act + assert
        Wrap.It(() => _ = evt.Item).Throws<InvalidOperationException>();
    }

    /// <summary>
    /// Accessing Items on a Set event throws InvalidOperationException.
    /// </summary>
    [Fact]
    public void Items_OnSetEvent_Throws()
    {
        // arrange
        var evt = ChangeEvent.Set(42);

        // act + assert
        Wrap.It(() => _ = evt.Items).Throws<InvalidOperationException>();
    }

    /// <summary>
    /// Accessing Items on a Delete event throws InvalidOperationException.
    /// </summary>
    [Fact]
    public void Items_OnDeleteEvent_Throws()
    {
        // arrange
        var evt = ChangeEvent.Delete(99);

        // act + assert
        Wrap.It(() => _ = evt.Items).Throws<InvalidOperationException>();
    }

    // ── Happy paths ───────────────────────────────────────────────────────────

    /// <summary>
    /// Items on an Init event returns the collection passed to ChangeEvent.Init.
    /// </summary>
    [Fact]
    public void Init_Items_ReturnsValues()
    {
        // arrange
        var values = new[] { 10, 20, 30 };
        var evt = ChangeEvent.Init(values);

        // act + assert
        evt.Type.Is(ChangeEventType.Init);
        evt.Items.Has(3);
        evt.Items.At(0).Is(10);
        evt.Items.At(1).Is(20);
        evt.Items.At(2).Is(30);
    }

    /// <summary>
    /// Item on a Set event returns the value passed to ChangeEvent.Set.
    /// </summary>
    [Fact]
    public void Set_Item_ReturnsValue()
    {
        // arrange
        var evt = ChangeEvent.Set(7);

        // act + assert
        evt.Type.Is(ChangeEventType.Set);
        evt.Item.Is(7);
    }
}
