using System;
using Annium.Testing;
using Xunit;

namespace Annium.Tests;

/// <summary>
/// Contains unit tests for <see cref="RandomExtensions"/>.
/// </summary>
public class RandomExtensionsTest
{
    /// <summary>
    /// Verifies that <see cref="RandomExtensions.NextBool"/> returns true at least once over a large sample.
    /// Mirrors the statistical-distribution style used by <c>EnumerableExtensionsTest.Shuffle_ReordersWithHighProbability</c>.
    /// The probability of all 1024 draws returning false on a fair 50/50 RNG is 2^-1024 — effectively impossible.
    /// </summary>
    [Fact]
    public void NextBool_DistributesBothValues()
    {
        // arrange
        var random = new Random(0xC0DE);
        var trueCount = 0;
        var falseCount = 0;

        // act
        for (var i = 0; i < 1024; i++)
        {
            if (random.NextBool())
                trueCount++;
            else
                falseCount++;
        }

        // assert
        trueCount.IsGreater(0);
        falseCount.IsGreater(0);
    }
}
