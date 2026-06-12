using System;
using System.Linq;
using Annium.Collections.Generic;
using Annium.Linq;
using Annium.Testing;
using Xunit;

namespace Annium.Tests.Collections.Generic;

/// <summary>
/// Contains unit tests for <see cref="SortedListSpan{TKey,TValue}"/> to verify sorted span behavior.
/// </summary>
public class SortedListSpanTest
{
    /// <summary>
    /// Verifies that adding elements, counting, indexing, and moving the sorted span work correctly.
    /// </summary>
    [Fact]
    public void Add_Count_Index_Move()
    {
        // arrange
        var data = Enumerable.Range(1, 5).ToSortedList(x => x - 1);

        // act & assert - move forward
        var span = data.ToListSpan(0, 2);
        span.Count.Is(2);
        span.Move(-1).IsFalse();
        span[0].Is(new(0, 1));
        span[1].Is(new(1, 2));
        span.Move(3).IsTrue();
        span[0].Is(new(3, 4));
        span[1].Is(new(4, 5));

        // act & assert - move backward
        span = data.ToListSpan(3, 2);
        span.Count.Is(2);
        span.Move(1).IsFalse();
        span[0].Is(new(3, 4));
        span[1].Is(new(4, 5));
        span.Move(-3).IsTrue();
        span[0].Is(new(0, 1));
        span[1].Is(new(1, 2));
    }

    /// <summary>Constructor throws ArgumentOutOfRangeException when start is negative.</summary>
    [Fact]
    public void Constructor_NegativeStart_Throws()
    {
        var data = Enumerable.Range(1, 3).ToSortedList(x => x - 1);

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new SortedListSpan<int, int>(data, -1, 2));
        Assert.Equal("start", ex.ParamName);
    }

    /// <summary>Constructor throws ArgumentOutOfRangeException when (start + count) exceeds the collection size.</summary>
    [Fact]
    public void Constructor_SpanOverrunsCollection_Throws()
    {
        var data = Enumerable.Range(1, 3).ToSortedList(x => x - 1);

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new SortedListSpan<int, int>(data, 2, 5));
        Assert.Equal("start", ex.ParamName);
    }

    /// <summary>Constructor accepts an empty span anchored at the end of the collection (start == count).</summary>
    [Fact]
    public void Constructor_EmptySpanAtEnd_DoesNotThrow()
    {
        var data = Enumerable.Range(1, 3).ToSortedList(x => x - 1);

        var span = new SortedListSpan<int, int>(data, 3, 0);

        span.Count.Is(0);
    }
}
