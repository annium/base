using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Testing;
using Xunit;

namespace Annium.Tests;

public class CopyableTest
{
    [Fact]
    public void Copyable_Base_Works()
    {
        // arrange
        var a = new A { W = 1, X = 2 };
        var b = new B { W = 3, Y = 4 };
        var l = new Base[] { a, b };
        var z = new Z { W = 7, Items = new List<Base> { a, b } };

        // act
        var ax = a.Copy();
        var bx = b.Copy();
        var lx = l.Select(x => x.Copy()).ToArray();
        var zx = z.Copy();

        // assert
        (ax == a).IsTrue();
        (bx == b).IsTrue();
        (lx[0] == l[0]).IsTrue();
        (lx[1] == l[1]).IsTrue();
        (zx == z).IsTrue();
        zx.W = 10;
        (zx == z).IsFalse();
        zx.W = 10;
        (zx == z).IsFalse();
        zx.Items[0].W = 10;
    }

    [Fact]
    public void Copyable_Complex_Works()
    {
        // arrange
        var a = new A { W = 1, X = 2 };
        var b = new B { W = 3, Y = 4 };
        var z = new Z { W = 7, Items = new List<Base> { a, b } };

        // act
        var zx = z.Copy();

        // assert
        // base
        (zx == z).IsTrue();

        // change direct field
        zx.W = 10;
        (zx == z).IsFalse();

        // change inner field
        zx.W = z.W;
        zx.Items[0] = zx.Items[0] with { W = 10 };
        (zx == z).IsFalse();

        // restore inner field
        zx.Items[0] = zx.Items[0] with { W = z.Items[0].W };
        (zx == z).IsTrue();
    }

    internal sealed record A : Base, ICopyable<A>
    {
        public int X { get; init; }
        public override A Copy() => this with { };
    }

    internal sealed record B : Base, ICopyable<B>
    {
        public int Y { get; init; }
        public override B Copy() => this with { };
    }

    internal sealed record Z : Base, ICopyable<Z>
    {
        public List<Base> Items { get; init; } = new();
        public override Z Copy() => this with { Items = Items.ToList() };
        public bool Equals(Z? other) => base.Equals(other) && Items.SequenceEqual(other.Items);
        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Items);
    }

    internal abstract record Base : ICopyable<Base>
    {
        public int W { get; set; }
        public abstract Base Copy();
    }
}