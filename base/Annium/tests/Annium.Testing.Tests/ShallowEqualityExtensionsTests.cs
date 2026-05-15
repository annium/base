using Xunit;

namespace Annium.Testing.Tests;

/// <summary>
/// Tests for <see cref="ShallowEqualityExtensions"/>. Uses xunit-native assertions because
/// Annium.Testing is itself the system under test.
/// </summary>
public class ShallowEqualityExtensionsTests
{
    private record Point(int X, int Y);

    /// <summary>Verifies IsEqual passes when shallow-equal records are compared.</summary>
    [Fact]
    public void IsEqual_PassesWhenShallowEqual()
    {
        var a = new Point(1, 2);
        var b = new Point(1, 2);

        var result = a.IsEqual(b);

        Assert.Same(a, result);
    }

    /// <summary>Verifies IsEqual throws when the values differ.</summary>
    [Fact]
    public void IsEqual_ThrowsWhenNotEqual()
    {
        var a = new Point(1, 2);
        var b = new Point(1, 3);

        Assert.Throws<AssertionFailedException>(() => a.IsEqual(b));
    }

    /// <summary>Verifies IsEqual respects the caller-supplied message.</summary>
    [Fact]
    public void IsEqual_UsesCustomMessage()
    {
        var a = new Point(1, 2);
        var b = new Point(1, 3);

        var ex = Assert.Throws<AssertionFailedException>(() => a.IsEqual(b, message: "custom"));

        Assert.Equal("custom", ex.Message);
    }

    /// <summary>Verifies IsNotEqual passes when the values differ.</summary>
    [Fact]
    public void IsNotEqual_PassesWhenNotEqual()
    {
        var a = new Point(1, 2);
        var b = new Point(1, 3);

        var result = a.IsNotEqual(b);

        Assert.Same(a, result);
    }

    /// <summary>Verifies IsNotEqual throws when the values are shallow-equal.</summary>
    [Fact]
    public void IsNotEqual_ThrowsWhenEqual()
    {
        var a = new Point(1, 2);
        var b = new Point(1, 2);

        Assert.Throws<AssertionFailedException>(() => a.IsNotEqual(b));
    }

    /// <summary>Verifies IsNotEqual respects the caller-supplied message.</summary>
    [Fact]
    public void IsNotEqual_UsesCustomMessage()
    {
        var a = new Point(1, 2);
        var b = new Point(1, 2);

        var ex = Assert.Throws<AssertionFailedException>(() => a.IsNotEqual(b, message: "boom"));

        Assert.Equal("boom", ex.Message);
    }
}
