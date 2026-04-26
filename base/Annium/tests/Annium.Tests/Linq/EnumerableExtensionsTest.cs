using System.Linq;
using Annium.Linq;
using Annium.Testing;
using Xunit;

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
            var shuffled = Annium.Linq.EnumerableExtensions.Shuffle(source).ToArray();
            if (!shuffled.SequenceEqual(source))
                nonIdentityRuns++;
        }

        // assert — with 100! possible orderings, identity ordering should be astronomically rare;
        // 90 % is a very lenient floor that still cleanly fails the Next(0, 1) regression (which
        // would produce 0 non-identity runs).
        nonIdentityRuns.IsGreaterOrEqual(900);
    }
}
