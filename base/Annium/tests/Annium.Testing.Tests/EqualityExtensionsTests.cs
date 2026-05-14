using System;
using Annium.Testing;
using Xunit;

namespace Annium.Testing.Tests;

/// <summary>
/// Tests for <see cref="EqualityExtensions"/>. Uses xunit-native assertions because Annium.Testing
/// is itself the system under test — testing the assertion library with itself would be circular.
/// </summary>
public class EqualityExtensionsTests
{
    /// <summary>Verifies that Is passes when values are equal.</summary>
    [Fact]
    public void Is_PassesWhenEqual()
    {
        // Must not throw.
        var result = 42.Is(42);
        Assert.Equal(42, result);
    }

    /// <summary>Verifies that Is throws AssertionFailedException when values differ.</summary>
    [Fact]
    public void Is_ThrowsWhenNotEqual()
    {
        Assert.Throws<AssertionFailedException>(() => 42.Is(43));
    }

    /// <summary>Verifies that IsNot passes when values differ.</summary>
    [Fact]
    public void IsNot_PassesWhenNotEqual()
    {
        var result = 42.IsNot(43);
        Assert.Equal(42, result);
    }

    /// <summary>Verifies that IsNot throws when values are equal.</summary>
    [Fact]
    public void IsNot_ThrowsWhenEqual()
    {
        Assert.Throws<AssertionFailedException>(() => 42.IsNot(42));
    }

    /// <summary>Verifies that IsDefault passes when value is null.</summary>
    [Fact]
    public void IsDefault_PassesWhenNull()
    {
        string? value = null;
        value.IsDefault();
    }

    /// <summary>Verifies that IsDefault throws when value is not null.</summary>
    [Fact]
    public void IsDefault_ThrowsWhenNotNull()
    {
        Assert.Throws<AssertionFailedException>(() => "x".IsDefault());
    }

    /// <summary>Verifies that IsNotDefault passes when value is non-null and returns the value.</summary>
    [Fact]
    public void IsNotDefault_PassesWhenNotNull()
    {
        string? value = "x";
        var result = value.IsNotDefault();
        Assert.Equal("x", result);
    }

    /// <summary>Verifies that IsNotDefault throws when value is null.</summary>
    [Fact]
    public void IsNotDefault_ThrowsWhenNull()
    {
        string? value = null;
        Assert.Throws<AssertionFailedException>(() => value.IsNotDefault());
    }

    /// <summary>Verifies that reference equality works for distinct-but-equal strings.</summary>
    [Fact]
    public void Is_StringEquality()
    {
        var a = new string("hello".ToCharArray());
        var b = new string("hello".ToCharArray());
        // These are distinct references but structurally equal — Is uses Equals.
        a.Is(b);
    }
}
