using System;
using System.Linq;
using Annium.Testing;
using Xunit;

namespace Annium.Data.Models.Tests;

/// <summary>
/// Tests for the Comparable implementation functionality.
/// </summary>
public class ComparableTest
{
    /// <summary>
    /// Tests that direct comparison operations work correctly for comparable objects.
    /// </summary>
    [Fact]
    public void DirectComparison_WorksCorrectly()
    {
        // arrange
        var a = new Money(1, 1);
        var b = new Money(1, 2);
        var c = new Money(2, 1);

        // assert
        a.CompareTo(null!).Is(1);
        Wrap.It(() => a.CompareTo(10)).Throws<ArgumentException>();
        a.CompareTo(a as object).Is(0);
        a.CompareTo(b as object).Is(-1);
        a.CompareTo(c as object).Is(-1);
        b.CompareTo(a as object).Is(1);
        b.CompareTo(c as object).Is(-1);
        c.CompareTo(a as object).Is(1);
        c.CompareTo(b as object).Is(1);
    }

    /// <summary>
    /// Tests that indirect comparison operations and operator overloads work correctly for comparable objects.
    /// </summary>
    [Fact]
    public void IndirectComparison_WorksCorrectly()
    {
        // arrange
        var a = new Money(1, 1);
        var d = a;
        var b = new Money(1, 2);
        var c = new Money(2, 1);

        // assert
        new[] { a, b, c, null }
            .Max()
            .Is(c);
        // reference comparison
        (a > d).IsFalse();
        (null! <= (null as Money)!).IsTrue();
        // >
        (a > b).IsFalse();
        (null! > a).IsFalse();
        (a > null!).IsTrue();
        // <
        (a < b).IsTrue();
        (null! < a).IsTrue();
        (a < null!).IsFalse();
        // >=
        (a >= b).IsFalse();
        (null! >= a).IsFalse();
        (a >= null!).IsTrue();
        // <=
        (a <= b).IsTrue();
        (null! <= a).IsTrue();
        (a <= null!).IsFalse();
    }
}
