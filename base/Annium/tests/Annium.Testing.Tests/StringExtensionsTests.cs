using Xunit;

namespace Annium.Testing.Tests;

/// <summary>
/// Tests for <see cref="StringExtensions"/>. Uses xunit-native assertions because Annium.Testing
/// is itself the system under test.
/// </summary>
public class StringExtensionsTests
{
    /// <summary>Verifies IsContaining passes when the substring is present.</summary>
    [Fact]
    public void IsContaining_Contains_Passes()
    {
        var result = "the quick brown fox".IsContaining("quick");

        Assert.Equal("the quick brown fox", result);
    }

    /// <summary>Verifies IsContaining throws when the substring is absent.</summary>
    [Fact]
    public void IsContaining_DoesNotContain_Throws()
    {
        Assert.Throws<AssertionFailedException>(() => "the quick".IsContaining("missing"));
    }

    /// <summary>Verifies IsContaining uses the caller-supplied message.</summary>
    [Fact]
    public void IsContaining_UsesCustomMessage()
    {
        var ex = Assert.Throws<AssertionFailedException>(() => "abc".IsContaining("z", message: "expected z"));

        Assert.Equal("expected z", ex.Message);
    }

    /// <summary>Verifies IsContainingAll passes when all substrings are present.</summary>
    [Fact]
    public void IsContainingAll_AllPresent_Passes()
    {
        var result = "alpha beta gamma".IsContainingAll(["alpha", "gamma"]);

        Assert.Equal("alpha beta gamma", result);
    }

    /// <summary>Verifies IsContainingAll throws when any substring is missing.</summary>
    [Fact]
    public void IsContainingAll_AnyMissing_Throws()
    {
        Assert.Throws<AssertionFailedException>(() => "alpha beta".IsContainingAll(["alpha", "missing"]));
    }

    /// <summary>Verifies IsContainingAll uses the caller-supplied message.</summary>
    [Fact]
    public void IsContainingAll_UsesCustomMessage()
    {
        var ex = Assert.Throws<AssertionFailedException>(() => "alpha".IsContainingAll(["beta"], message: "need beta"));

        Assert.Equal("need beta", ex.Message);
    }

    /// <summary>Verifies IsNotContaining passes when the substring is absent.</summary>
    [Fact]
    public void IsNotContaining_Absent_Passes()
    {
        var result = "alpha beta".IsNotContaining("gamma");

        Assert.Equal("alpha beta", result);
    }

    /// <summary>Verifies IsNotContaining throws when the substring is present.</summary>
    [Fact]
    public void IsNotContaining_Present_Throws()
    {
        Assert.Throws<AssertionFailedException>(() => "alpha beta".IsNotContaining("alpha"));
    }

    /// <summary>Verifies IsNotContaining uses the caller-supplied message.</summary>
    [Fact]
    public void IsNotContaining_UsesCustomMessage()
    {
        var ex = Assert.Throws<AssertionFailedException>(() =>
            "alpha".IsNotContaining("alpha", message: "should not contain alpha")
        );

        Assert.Equal("should not contain alpha", ex.Message);
    }
}
