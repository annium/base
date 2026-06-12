using Xunit;

namespace Annium.Testing.Tests;

/// <summary>Tests for <see cref="BooleanExtensions"/>.</summary>
public class BooleanExtensionsTests
{
    /// <summary>Verifies that IsTrue passes when the value is true.</summary>
    [Fact]
    public void IsTrue_PassesWhenTrue() => true.IsTrue();

    /// <summary>Verifies that IsTrue throws when the value is false.</summary>
    [Fact]
    public void IsTrue_ThrowsWhenFalse()
    {
        Assert.Throws<AssertionFailedException>(() => false.IsTrue());
    }

    /// <summary>Verifies that IsFalse passes when the value is false.</summary>
    [Fact]
    public void IsFalse_PassesWhenFalse() => false.IsFalse();

    /// <summary>Verifies that IsFalse throws when the value is true.</summary>
    [Fact]
    public void IsFalse_ThrowsWhenTrue()
    {
        Assert.Throws<AssertionFailedException>(() => true.IsFalse());
    }
}
