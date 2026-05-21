using System.Linq;
using Annium.Linq;
using Annium.Testing;
using Xunit;
using EnumerableExtensions = Annium.Linq.EnumerableExtensions;

namespace Annium.Tests.Linq;

/// <summary>
/// Contains unit tests for Enumerable extension methods.
/// </summary>
public class EnumerableExtensionsTest
{
    /// <summary>
    /// Verifies that CartesianProduct produces the correct cartesian product of input arrays.
    /// </summary>
    [Fact]
    public void CartesianProduct()
    {
        // arrange
        var a = new[] { 1, 2 };
        var b = new[] { 3, 4 };

        // act
        var result = new[] { a, b }.CartesianProduct();

        // assert
        result.IsEqual(new[] { new[] { 1, 3 }, new[] { 1, 4 }, new[] { 2, 3 }, new[] { 2, 4 } });
    }

    /// <summary>
    /// Regression test for <c>Shuffle</c>. Previously the ordering key was
    /// <c>_random.Next(0, 1) == 1</c>, which always evaluated to <c>false</c> because
    /// <see cref="System.Random.Next(int, int)"/> has an exclusive upper bound — so the stable
    /// sort produced an identity ordering every time. A correct implementation moves elements
    /// around most of the time; this test asserts a lower-bound of 90 % non-identity runs over
    /// 1000 shuffles of a 100-element sequence.
    /// </summary>
    [Fact]
    public void Shuffle_ReordersWithHighProbability()
    {
        // arrange
        var source = Enumerable.Range(0, 100).ToArray();

        // act — call Annium's Shuffle explicitly; .NET 10's System.Linq.Enumerable.Shuffle would
        // otherwise ambiguate the extension-method resolution.
        var nonIdentityRuns = 0;
        for (var i = 0; i < 1000; i++)
        {
            var shuffled = EnumerableExtensions.Shuffle(source).ToArray();
            if (!shuffled.SequenceEqual(source))
                nonIdentityRuns++;
        }

        // assert — with 100! possible orderings, identity ordering should be astronomically rare;
        // 90 % is a very lenient floor that still cleanly fails the Next(0, 1) regression (which
        // would produce 0 non-identity runs).
        nonIdentityRuns.IsGreaterOrEqual(900);
    }

    /// <summary>Yield wraps a non-null value into a single-element sequence.</summary>
    [Fact]
    public void Yield_NonNull_ReturnsSingleElement()
    {
        var result = "hello".Yield().ToArray();

        result.IsEqual(new[] { "hello" });
    }

    /// <summary>Yield on a null reference returns an empty sequence.</summary>
    [Fact]
    public void Yield_Null_ReturnsEmpty()
    {
        string? source = null;

        var result = source!.Yield().ToArray();

        result.Length.Is(0);
    }

    /// <summary>WhereNot keeps elements for which the predicate is false.</summary>
    [Fact]
    public void WhereNot_KeepsNonMatching()
    {
        var src = new[] { 1, 2, 3, 4 };

        var result = src.WhereNot(x => x % 2 == 0).ToArray();

        result.IsEqual(new[] { 1, 3 });
    }

    /// <summary>None returns true when no element matches the predicate.</summary>
    [Fact]
    public void None_NoMatch_ReturnsTrue()
    {
        var src = new[] { 1, 3, 5 };

        src.None(x => x % 2 == 0).IsTrue();
    }

    /// <summary>None returns false when any element matches the predicate.</summary>
    [Fact]
    public void None_AnyMatch_ReturnsFalse()
    {
        var src = new[] { 1, 2, 3 };

        src.None(x => x % 2 == 0).IsFalse();
    }

    /// <summary>Join with the default empty separator concatenates strings tightly.</summary>
    [Fact]
    public void Join_DefaultSeparator_ConcatsTight()
    {
        var src = new[] { "a", "b", "c" };

        src.Join().Is("abc");
    }

    /// <summary>Join with a separator interleaves the separator between elements.</summary>
    [Fact]
    public void Join_WithSeparator_Interleaves()
    {
        var src = new[] { "a", "b", "c" };

        src.Join(", ").Is("a, b, c");
    }
}
