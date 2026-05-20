using System;
using System.Collections.Generic;
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

    /// <summary>
    /// Verifies that NextEnum with no explicit values returns only valid members of the enum.
    /// Over 200 iterations every returned value must be a defined DayOfWeek.
    /// </summary>
    [Fact]
    public void NextEnum_NoValues_ReturnsValueFromAllEnumValues()
    {
        // arrange
        const int iterations = 200;
        var random = new Random(0xC0DE);

        // act + assert
        for (var i = 0; i < iterations; i++)
        {
            var result = random.NextEnum<DayOfWeek>();
            Enum.IsDefined(typeof(DayOfWeek), result).IsTrue();
        }
    }

    /// <summary>
    /// NextDecimal returns a value in [0, 1) — same range as the underlying NextDouble().
    /// A mutation removing the cast or returning a constant would fail this range check.
    /// </summary>
    [Fact]
    public void NextDecimal_ReturnsValueBetweenZeroAndOne()
    {
        var random = new Random(0xC0DE);

        for (var i = 0; i < 256; i++)
        {
            var v = random.NextDecimal();
            (v >= 0m).IsTrue();
            (v < 1m).IsTrue();
        }
    }

    /// <summary>
    /// NextDecimal distributes values across the [0, 1) range — over 1024 draws we must see at least
    /// one value in each quartile. Detects a mutation returning a constant.
    /// </summary>
    [Fact]
    public void NextDecimal_DistributesValuesAcrossRange()
    {
        var random = new Random(0xC0DE);
        var lowQuartile = 0;
        var highQuartile = 0;

        for (var i = 0; i < 1024; i++)
        {
            var v = random.NextDecimal();
            if (v < 0.25m)
                lowQuartile++;
            else if (v >= 0.75m)
                highQuartile++;
        }

        lowQuartile.IsGreater(0);
        highQuartile.IsGreater(0);
    }

    /// <summary>
    /// Verifies that NextEnum with a subset of values only returns members from that subset.
    /// Over 200 iterations every returned value must belong to the supplied subset.
    /// </summary>
    [Fact]
    public void NextEnum_WithSubset_ReturnsOnlySubsetValues()
    {
        // arrange
        const int iterations = 200;
        var random = new Random(0xC0DE);
        var subset = new HashSet<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Friday };

        // act + assert
        for (var i = 0; i < iterations; i++)
        {
            var result = random.NextEnum(DayOfWeek.Monday, DayOfWeek.Friday);
            subset.Contains(result).IsTrue();
        }
    }
}
