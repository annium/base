using System;
using Xunit;

namespace Annium.Testing.Tests;

/// <summary>
/// Tests for <see cref="ArrayExtensions"/>, including the custom-message overloads that mirror
/// <see cref="EnumerableExtensions"/> and <see cref="DictionaryExtensions"/> conventions.
/// </summary>
public class ArrayExtensionsTests
{
    /// <summary>At returns the element at the given index.</summary>
    [Fact]
    public void At_IndexInRange_ReturnsElement()
    {
        var arr = new[] { 10, 20, 30 };

        var result = arr.At(1);

        Assert.Equal(20, result);
    }

    /// <summary>At throws when the index is out of bounds.</summary>
    [Fact]
    public void At_IndexOutOfBounds_Throws()
    {
        var arr = new[] { 10, 20, 30 };

        Assert.Throws<AssertionFailedException>(() => arr.At(5));
    }

    /// <summary>At throws ArgumentNullException when the array is null.</summary>
    [Fact]
    public void At_NullArray_ThrowsArgumentNull()
    {
        int[] arr = null!;

        Assert.Throws<ArgumentNullException>(() => arr.At(0));
    }

    /// <summary>Has passes when the count matches.</summary>
    [Fact]
    public void Has_CountMatches_Passes()
    {
        var arr = new[] { 1, 2, 3 };

        var result = arr.Has(3);

        Assert.Same(arr, result);
    }

    /// <summary>Has throws when the count differs.</summary>
    [Fact]
    public void Has_CountDiffers_Throws()
    {
        var arr = new[] { 1, 2 };

        Assert.Throws<AssertionFailedException>(() => arr.Has(99));
    }

    /// <summary>Has(count, message) puts the custom message into the thrown exception.</summary>
    [Fact]
    public void Has_CustomMessage_AppearsInException()
    {
        var ex = Assert.Throws<AssertionFailedException>(() => new[] { 1, 2 }.Has(3, "domain count msg"));
        Assert.Equal("domain count msg", ex.Message);
    }

    /// <summary>IsEmpty passes for an empty array.</summary>
    [Fact]
    public void IsEmpty_EmptyArray_Passes()
    {
        var arr = Array.Empty<int>();

        var result = arr.IsEmpty();

        Assert.Same(arr, result);
    }

    /// <summary>IsEmpty throws for a non-empty array.</summary>
    [Fact]
    public void IsEmpty_NonEmpty_Throws()
    {
        Assert.Throws<AssertionFailedException>(() => new[] { 1 }.IsEmpty());
    }

    /// <summary>IsEmpty(message) puts the custom message into the thrown exception.</summary>
    [Fact]
    public void IsEmpty_CustomMessage_AppearsInException()
    {
        var ex = Assert.Throws<AssertionFailedException>(() => new[] { 1 }.IsEmpty("expected nothing"));
        Assert.Equal("expected nothing", ex.Message);
    }

    /// <summary>IsNotEmpty passes for a non-empty array.</summary>
    [Fact]
    public void IsNotEmpty_NonEmpty_Passes()
    {
        var arr = new[] { 1 };

        var result = arr.IsNotEmpty();

        Assert.Same(arr, result);
    }

    /// <summary>IsNotEmpty throws for an empty array.</summary>
    [Fact]
    public void IsNotEmpty_Empty_Throws()
    {
        Assert.Throws<AssertionFailedException>(() => Array.Empty<int>().IsNotEmpty());
    }

    /// <summary>IsNotEmpty(message) puts the custom message into the thrown exception.</summary>
    [Fact]
    public void IsNotEmpty_CustomMessage_AppearsInException()
    {
        var ex = Assert.Throws<AssertionFailedException>(() => Array.Empty<int>().IsNotEmpty("need at least one"));
        Assert.Equal("need at least one", ex.Message);
    }
}
