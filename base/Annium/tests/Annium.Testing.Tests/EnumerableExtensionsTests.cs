using System.Collections.Generic;
using Annium.Testing;
using Xunit;

namespace Annium.Testing.Tests;

/// <summary>Tests for <see cref="EnumerableExtensions"/> and related collection assertions.</summary>
public class EnumerableExtensionsTests
{
    /// <summary>Has(count) passes when count matches.</summary>
    [Fact]
    public void Has_PassesWhenCountMatches() => new List<int> { 1, 2, 3 }.Has(3);

    /// <summary>Has(count) throws when count differs.</summary>
    [Fact]
    public void Has_ThrowsWhenCountDiffers()
    {
        Assert.Throws<AssertionFailedException>(() => new List<int> { 1, 2 }.Has(3));
    }

    /// <summary>IsEmpty passes for an empty enumerable.</summary>
    [Fact]
    public void IsEmpty_PassesForEmpty() => new List<int>().IsEmpty();

    /// <summary>IsEmpty throws for a non-empty enumerable.</summary>
    [Fact]
    public void IsEmpty_ThrowsForNonEmpty()
    {
        Assert.Throws<AssertionFailedException>(() => new List<int> { 1 }.IsEmpty());
    }

    /// <summary>IsNotEmpty passes for non-empty.</summary>
    [Fact]
    public void IsNotEmpty_PassesForNonEmpty() => new List<int> { 1 }.IsNotEmpty();

    /// <summary>IsNotEmpty throws for empty.</summary>
    [Fact]
    public void IsNotEmpty_ThrowsForEmpty()
    {
        Assert.Throws<AssertionFailedException>(() => new List<int>().IsNotEmpty());
    }

    /// <summary>At returns element at index.</summary>
    [Fact]
    public void At_ReturnsElementAtIndex()
    {
        var arr = new[] { 10, 20, 30 };
        Assert.Equal(20, arr.At(1));
    }
}
