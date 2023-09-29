using Annium.Testing;
using Xunit;

// ReSharper disable AccessToModifiedClosure

namespace Annium.Data.Models.Tests;

public class ValueRangeTest
{
    [Fact]
    public void Managed_Basics()
    {
        // arrange
        var range = ValueRange.Create(1, 2);

        // assert
        range.Start.Is(1);
        range.End.Is(2);

        // act
        range.SetStart(3);
        range.SetEnd(4);

        // assert
        range.Start.Is(3);
        range.End.Is(4);
    }

    [Fact]
    public void Computed_Basics()
    {
        // arrange
        var x = 1;
        var y = 2;
        var range = ValueRange.Create(() => x, () => y);

        // assert
        range.Start.Is(1);
        range.End.Is(2);

        // act
        x = 3;
        y = 4;

        // assert
        range.Start.Is(3);
        range.End.Is(4);
    }

    [Fact]
    public void Contains_Point()
    {
        // arrange
        var range = ValueRange.Create(1, 3);

        // assert
        range.Contains(1, RangeBounds.None).IsFalse();
        range.Contains(1, RangeBounds.Start).IsTrue();
        range.Contains(1, RangeBounds.End).IsFalse();
        range.Contains(1, RangeBounds.Both).IsTrue();
        range.Contains(2, RangeBounds.None).IsTrue();
        range.Contains(2, RangeBounds.Start).IsTrue();
        range.Contains(2, RangeBounds.End).IsTrue();
        range.Contains(2, RangeBounds.Both).IsTrue();
        range.Contains(3, RangeBounds.None).IsFalse();
        range.Contains(3, RangeBounds.Start).IsFalse();
        range.Contains(3, RangeBounds.End).IsTrue();
        range.Contains(3, RangeBounds.Both).IsTrue();
    }

    [Fact]
    public void Contains_Range()
    {
        // arrange
        var range = ValueRange.Create(1, 4);

        // assert
        range.Contains(ValueRange.Create(1, 4), RangeBounds.None).IsFalse();
        range.Contains(ValueRange.Create(1, 4), RangeBounds.Start).IsFalse();
        range.Contains(ValueRange.Create(1, 4), RangeBounds.End).IsFalse();
        range.Contains(ValueRange.Create(1, 4), RangeBounds.Both).IsTrue();
        range.Contains(ValueRange.Create(1, 3), RangeBounds.None).IsFalse();
        range.Contains(ValueRange.Create(1, 3), RangeBounds.Start).IsTrue();
        range.Contains(ValueRange.Create(1, 3), RangeBounds.End).IsFalse();
        range.Contains(ValueRange.Create(1, 3), RangeBounds.Both).IsTrue();
        range.Contains(ValueRange.Create(2, 4), RangeBounds.None).IsFalse();
        range.Contains(ValueRange.Create(2, 4), RangeBounds.Start).IsFalse();
        range.Contains(ValueRange.Create(2, 4), RangeBounds.End).IsTrue();
        range.Contains(ValueRange.Create(2, 4), RangeBounds.Both).IsTrue();
        range.Contains(ValueRange.Create(2, 3), RangeBounds.None).IsTrue();
        range.Contains(ValueRange.Create(2, 3), RangeBounds.Start).IsTrue();
        range.Contains(ValueRange.Create(2, 3), RangeBounds.End).IsTrue();
        range.Contains(ValueRange.Create(2, 3), RangeBounds.Both).IsTrue();
    }

    [Fact]
    public void Subtract()
    {
        // arrange
        var range = ValueRange.Create(1, 4);

        // assert
        // SS TS TE SE -> SS TS, TE SE
        var diff = range - ValueRange.Create(2, 3);
        diff.Has(2);
        diff.At(0).Is(ValueRange.Create(1, 2));
        diff.At(1).Is(ValueRange.Create(3, 4));
        // TS SS SE TE -> []
        (range - ValueRange.Create(1, 4)).IsEmpty();
        // TS SS TE SE-> TE SE
        (range - ValueRange.Create(1, 2)).Has(1).At(0).Is(ValueRange.Create(2, 4));
        // SS TS SE TE -> SS TS
        (range - ValueRange.Create(3, 4)).Has(1).At(0).Is(ValueRange.Create(1, 3));
        // SS SE TS TE -> SS SE
        (range - ValueRange.Create(0, 1)).Has(1).At(0).Is(ValueRange.Create(1, 4));
        (range - ValueRange.Create(4, 5)).Has(1).At(0).Is(ValueRange.Create(1, 4));
    }
}