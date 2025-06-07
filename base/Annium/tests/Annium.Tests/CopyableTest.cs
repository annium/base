using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Testing;
using Xunit;

namespace Annium.Tests;

/// <summary>
/// Contains unit tests for <see cref="ICopyable{T}"/> to verify copying behavior.
/// </summary>
public class CopyableTest
{
    /// <summary>
    /// Verifies that copying base objects works correctly.
    /// </summary>
    [Fact]
    public void Copyable_Base_Works()
    {
        // arrange
        var a = new A { W = 1, X = 2 };
        var b = new B { W = 3, Y = 4 };
        var l = new Base[] { a, b };
        var z = new Z
        {
            W = 7,
            Items = new List<Base> { a, b },
        };

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

    /// <summary>
    /// Verifies that copying complex objects works correctly.
    /// </summary>
    [Fact]
    public void Copyable_Complex_Works()
    {
        // arrange
        var a = new A { W = 1, X = 2 };
        var b = new B { W = 3, Y = 4 };
        var z = new Z
        {
            W = 7,
            Items = new List<Base> { a, b },
        };

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
        zx.Items[0] = zx.Items[0] with
        {
            W = z.Items[0].W,
        };
        (zx == z).IsTrue();
    }

    /// <summary>
    /// Represents a test record A, derived from Base, implementing ICopyable.
    /// </summary>
    internal sealed record A : Base, ICopyable<A>
    {
        /// <summary>
        /// Gets or sets the X value.
        /// </summary>
        public int X { get; init; }

        /// <summary>
        /// Creates a copy of the current object.
        /// </summary>
        /// <returns>A new instance of A with the same values.</returns>
        public override A Copy() => this with { };
    }

    /// <summary>
    /// Represents a test record B, derived from Base, implementing ICopyable.
    /// </summary>
    internal sealed record B : Base, ICopyable<B>
    {
        /// <summary>
        /// Gets or sets the Y value.
        /// </summary>
        public int Y { get; init; }

        /// <summary>
        /// Creates a copy of the current object.
        /// </summary>
        /// <returns>A new instance of B with the same values.</returns>
        public override B Copy() => this with { };
    }

    /// <summary>
    /// Represents a test record Z, derived from Base, implementing ICopyable, with a list of Base items.
    /// </summary>
    internal sealed record Z : Base, ICopyable<Z>
    {
        /// <summary>
        /// Gets or sets the list of Base items.
        /// </summary>
        public List<Base> Items { get; init; } = new();

        /// <summary>
        /// Creates a copy of the current object.
        /// </summary>
        /// <returns>A new instance of Z with the same values and a new list containing copies of the items.</returns>
        public override Z Copy() => this with { Items = Items.ToList() };

        /// <summary>
        /// Determines whether the specified Z is equal to the current Z.
        /// </summary>
        /// <param name="other">The Z to compare with the current Z.</param>
        /// <returns>true if the specified Z is equal to the current Z; otherwise, false.</returns>
        public bool Equals(Z? other) => base.Equals(other) && Items.SequenceEqual(other.Items);

        /// <summary>
        /// Gets the hash code for the current Z.
        /// </summary>
        /// <returns>A hash code for the current Z.</returns>
        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Items);
    }

    /// <summary>
    /// Represents a base record for testing, implementing ICopyable.
    /// </summary>
    internal abstract record Base : ICopyable<Base>
    {
        /// <summary>
        /// Gets or sets the W value.
        /// </summary>
        public int W { get; set; }

        /// <summary>
        /// Creates a copy of the current object.
        /// </summary>
        /// <returns>A new instance of Base with the same values.</returns>
        public abstract Base Copy();
    }
}
