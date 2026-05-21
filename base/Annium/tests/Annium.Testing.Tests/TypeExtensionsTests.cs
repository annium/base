using System;
using Xunit;

namespace Annium.Testing.Tests;

/// <summary>Tests for <see cref="TypeExtensions"/> — type-assertion helpers.</summary>
public class TypeExtensionsTests
{
    /// <summary>As&lt;T&gt; returns the cast value when the type matches.</summary>
    [Fact]
    public void As_PassesWhenTypeMatches()
    {
        object value = new InvalidOperationException("x");
        var typed = value.As<InvalidOperationException>();
        Assert.Equal("x", typed.Message);
    }

    /// <summary>As&lt;T&gt; passes when the value is a derived type (is-check semantics).</summary>
    [Fact]
    public void As_PassesForDerivedType()
    {
        Exception value = new InvalidOperationException("x");
        var typed = value.As<Exception>();
        Assert.Equal("x", typed.Message);
    }

    /// <summary>As&lt;T&gt; throws when the value is not of the expected type.</summary>
    [Fact]
    public void As_ThrowsWhenTypeMismatch()
    {
        object value = "not an exception";
        Assert.Throws<AssertionFailedException>(() => value.As<Exception>());
    }

    /// <summary>AsExact&lt;T&gt; passes only when the runtime type matches exactly.</summary>
    [Fact]
    public void AsExact_PassesWhenExactMatch()
    {
        object value = new InvalidOperationException("x");
        var typed = value.AsExact<InvalidOperationException>();
        Assert.Equal("x", typed.Message);
    }

    /// <summary>AsExact&lt;T&gt; throws for a derived type (unlike As).</summary>
    [Fact]
    public void AsExact_ThrowsForDerivedType()
    {
        object value = new InvalidOperationException("x");
        Assert.Throws<AssertionFailedException>(() => value.AsExact<Exception>());
    }
}
