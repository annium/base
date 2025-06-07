using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Testing;
using Xunit;

namespace Annium.Tests;

/// <summary>
/// Contains unit tests for verifying hash code sequence behavior.
/// </summary>
public class HashCodeSeqTest
{
    /// <summary>
    /// Verifies that complex copyable objects produce the same hash code when their contents are equal.
    /// </summary>
    [Fact]
    public void Copyable_Complex_Works()
    {
        // arrange
        var a = new A { W = 1, X = 2 };
        var b = new B { W = 3, Y = 4 };
        var x = new Z
        {
            W = 7,
            Items = new List<Base> { a, b },
        };
        var y = new Z
        {
            W = 7,
            Items = new List<Base> { a, b },
        };

        // assert
        x.GetHashCode().Is(y.GetHashCode());
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
        /// Creates a copy of the current A record.
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
        /// Creates a copy of the current B record.
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
        /// Creates a copy of the current Z record with a new list containing the same items.
        /// </summary>
        /// <returns>A new instance of Z with the same values and a new list containing the same items.</returns>
        public override Z Copy() => this with { Items = Items.ToList() };

        /// <summary>
        /// Determines whether the specified Z is equal to the current Z.
        /// </summary>
        /// <param name="other">The Z to compare with the current Z.</param>
        /// <returns>true if the specified Z is equal to the current Z; otherwise, false.</returns>
        public bool Equals(Z? other) => base.Equals(other) && Items.SequenceEqual(other.Items);

        /// <summary>
        /// Gets the hash code for the current Z record.
        /// </summary>
        /// <returns>A hash code for the current Z record.</returns>
        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), HashCodeSeq.Combine(Items));
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
        /// Creates a copy of the current Base record.
        /// </summary>
        /// <returns>A new instance of Base with the same values.</returns>
        public abstract Base Copy();
    }
}
