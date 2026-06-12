using Xunit;

namespace Annium.Testing.Tests;

/// <summary>Tests for <see cref="ComparisonExtensions"/>.</summary>
public class ComparisonExtensionsTests
{
    /// <summary>IsLess passes when value is strictly less than other.</summary>
    [Fact]
    public void IsLess_PassesWhenStrictlyLess() => 1.IsLess(2);

    /// <summary>IsLess throws on equality.</summary>
    [Fact]
    public void IsLess_ThrowsWhenEqual()
    {
        Assert.Throws<AssertionFailedException>(() => 2.IsLess(2));
    }

    /// <summary>IsLess throws when value is greater.</summary>
    [Fact]
    public void IsLess_ThrowsWhenGreater()
    {
        Assert.Throws<AssertionFailedException>(() => 3.IsLess(2));
    }

    /// <summary>IsGreater passes when strictly greater.</summary>
    [Fact]
    public void IsGreater_PassesWhenStrictlyGreater() => 3.IsGreater(2);

    /// <summary>IsGreater throws on equality.</summary>
    [Fact]
    public void IsGreater_ThrowsWhenEqual()
    {
        Assert.Throws<AssertionFailedException>(() => 2.IsGreater(2));
    }

    /// <summary>IsLessOrEqual passes for equal values.</summary>
    [Fact]
    public void IsLessOrEqual_PassesWhenEqual() => 2.IsLessOrEqual(2);

    /// <summary>IsLessOrEqual passes when strictly less.</summary>
    [Fact]
    public void IsLessOrEqual_PassesWhenLess() => 1.IsLessOrEqual(2);

    /// <summary>IsLessOrEqual throws when greater.</summary>
    [Fact]
    public void IsLessOrEqual_ThrowsWhenGreater()
    {
        Assert.Throws<AssertionFailedException>(() => 3.IsLessOrEqual(2));
    }

    /// <summary>IsGreaterOrEqual passes for equal values.</summary>
    [Fact]
    public void IsGreaterOrEqual_PassesWhenEqual() => 2.IsGreaterOrEqual(2);

    /// <summary>IsGreaterOrEqual throws when less.</summary>
    [Fact]
    public void IsGreaterOrEqual_ThrowsWhenLess()
    {
        Assert.Throws<AssertionFailedException>(() => 1.IsGreaterOrEqual(2));
    }
}
