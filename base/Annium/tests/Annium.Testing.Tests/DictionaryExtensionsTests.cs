using System.Collections.Generic;
using Xunit;

namespace Annium.Testing.Tests;

/// <summary>
/// Tests for <see cref="DictionaryExtensions"/> on both <see cref="IDictionary{TKey,TValue}"/> and
/// <see cref="IReadOnlyDictionary{TKey,TValue}"/>. Uses xunit-native assertions to avoid circular
/// dependency on Annium.Testing itself.
/// </summary>
public class DictionaryExtensionsTests
{
    private static IDictionary<string, int> MutableSample => new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };
    private static IReadOnlyDictionary<string, int> ReadOnlySample => new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };

    /// <summary>Verifies At returns the value for an existing key on IDictionary.</summary>
    [Fact]
    public void At_IDictionary_KeyExists_ReturnsValue()
    {
        var dict = MutableSample;

        var value = dict.At("a");

        Assert.Equal(1, value);
    }

    /// <summary>Verifies At throws when the key is missing on IDictionary.</summary>
    [Fact]
    public void At_IDictionary_KeyMissing_Throws()
    {
        var dict = MutableSample;

        Assert.Throws<AssertionFailedException>(() => dict.At("missing"));
    }

    /// <summary>Verifies At returns the value for an existing key on IReadOnlyDictionary.</summary>
    [Fact]
    public void At_IReadOnlyDictionary_KeyExists_ReturnsValue()
    {
        var dict = ReadOnlySample;

        var value = dict.At("b");

        Assert.Equal(2, value);
    }

    /// <summary>Verifies At throws when the key is missing on IReadOnlyDictionary.</summary>
    [Fact]
    public void At_IReadOnlyDictionary_KeyMissing_Throws()
    {
        var dict = ReadOnlySample;

        Assert.Throws<AssertionFailedException>(() => dict.At("missing"));
    }

    /// <summary>Verifies Has passes when the count matches on IDictionary.</summary>
    [Fact]
    public void Has_IDictionary_CountMatches_Passes()
    {
        var dict = MutableSample;

        var result = dict.Has(2);

        Assert.Same(dict, result);
    }

    /// <summary>Verifies Has throws when the count differs on IDictionary.</summary>
    [Fact]
    public void Has_IDictionary_CountDiffers_Throws()
    {
        var dict = MutableSample;

        Assert.Throws<AssertionFailedException>(() => dict.Has(99));
    }

    /// <summary>Verifies Has passes when the count matches on IReadOnlyDictionary.</summary>
    [Fact]
    public void Has_IReadOnlyDictionary_CountMatches_Passes()
    {
        var dict = ReadOnlySample;

        var result = dict.Has(2);

        Assert.Same(dict, result);
    }

    /// <summary>Verifies Has throws when the count differs on IReadOnlyDictionary.</summary>
    [Fact]
    public void Has_IReadOnlyDictionary_CountDiffers_Throws()
    {
        var dict = ReadOnlySample;

        Assert.Throws<AssertionFailedException>(() => dict.Has(99));
    }

    /// <summary>Verifies IsEmpty passes when IDictionary has zero items.</summary>
    [Fact]
    public void IsEmpty_IDictionary_Passes()
    {
        IDictionary<string, int> dict = new Dictionary<string, int>();

        var result = dict.IsEmpty();

        Assert.Same(dict, result);
    }

    /// <summary>Verifies IsEmpty throws when IDictionary is non-empty.</summary>
    [Fact]
    public void IsEmpty_IDictionary_NonEmpty_Throws()
    {
        var dict = MutableSample;

        Assert.Throws<AssertionFailedException>(() => dict.IsEmpty());
    }

    /// <summary>Verifies IsEmpty passes when IReadOnlyDictionary has zero items.</summary>
    [Fact]
    public void IsEmpty_IReadOnlyDictionary_Passes()
    {
        IReadOnlyDictionary<string, int> dict = new Dictionary<string, int>();

        var result = dict.IsEmpty();

        Assert.Same(dict, result);
    }

    /// <summary>Verifies IsEmpty throws when IReadOnlyDictionary is non-empty.</summary>
    [Fact]
    public void IsEmpty_IReadOnlyDictionary_NonEmpty_Throws()
    {
        var dict = ReadOnlySample;

        Assert.Throws<AssertionFailedException>(() => dict.IsEmpty());
    }

    /// <summary>Verifies IsNotEmpty passes when IDictionary has items.</summary>
    [Fact]
    public void IsNotEmpty_IDictionary_Passes()
    {
        var dict = MutableSample;

        var result = dict.IsNotEmpty();

        Assert.Same(dict, result);
    }

    /// <summary>Verifies IsNotEmpty throws when IDictionary is empty.</summary>
    [Fact]
    public void IsNotEmpty_IDictionary_Empty_Throws()
    {
        IDictionary<string, int> dict = new Dictionary<string, int>();

        Assert.Throws<AssertionFailedException>(() => dict.IsNotEmpty());
    }

    /// <summary>Verifies IsNotEmpty passes when IReadOnlyDictionary has items.</summary>
    [Fact]
    public void IsNotEmpty_IReadOnlyDictionary_Passes()
    {
        var dict = ReadOnlySample;

        var result = dict.IsNotEmpty();

        Assert.Same(dict, result);
    }

    /// <summary>Verifies IsNotEmpty throws when IReadOnlyDictionary is empty.</summary>
    [Fact]
    public void IsNotEmpty_IReadOnlyDictionary_Empty_Throws()
    {
        IReadOnlyDictionary<string, int> dict = new Dictionary<string, int>();

        Assert.Throws<AssertionFailedException>(() => dict.IsNotEmpty());
    }
}
