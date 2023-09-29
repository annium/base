using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Testing;
using Xunit;

namespace Annium.Tests;

public class HashCodeSeqTest
{
    [Fact]
    public void Copyable_Complex_Works()
    {
        // arrange
        var a = new A { W = 1, X = 2 };
        var b = new B { W = 3, Y = 4 };
        var x = new Z { W = 7, Items = new List<Base> { a, b } };
        var y = new Z { W = 7, Items = new List<Base> { a, b } };

        // assert
        x.GetHashCode().Is(y.GetHashCode());
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
        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), HashCodeSeq.Combine(Items));
    }

    internal abstract record Base : ICopyable<Base>
    {
        public int W { get; set; }
        public abstract Base Copy();
    }
}