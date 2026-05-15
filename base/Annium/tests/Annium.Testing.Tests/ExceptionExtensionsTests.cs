#pragma warning disable VSTHRD003 // Tests legitimately await Task.FromResult to exercise async overloads.

using System;
using System.Threading.Tasks;
using Xunit;

namespace Annium.Testing.Tests;

/// <summary>
/// Tests for <see cref="ExceptionExtensions"/>. Uses xunit-native assertions to avoid circular
/// dependency on Annium.Testing.
/// </summary>
public class ExceptionExtensionsTests
{
    private static InvalidOperationException Sample(string message) => new(message);

    /// <summary>Verifies Reports passes when the message contains the expected text.</summary>
    [Fact]
    public void Reports_Contains_Passes()
    {
        var ex = Sample("the quick brown fox");

        var result = ex.Reports("quick brown");

        Assert.Same(ex, result);
    }

    /// <summary>Verifies Reports throws when the message does not contain the expected text.</summary>
    [Fact]
    public void Reports_DoesNotContain_Throws()
    {
        var ex = Sample("hello");

        Assert.Throws<AssertionFailedException>(() => ex.Reports("missing"));
    }

    /// <summary>Verifies ReportsAll passes when all texts are contained.</summary>
    [Fact]
    public void ReportsAll_AllPresent_Passes()
    {
        var ex = Sample("alpha beta gamma");

        var result = ex.ReportsAll(["alpha", "gamma"]);

        Assert.Same(ex, result);
    }

    /// <summary>Verifies ReportsAll throws when any text is missing.</summary>
    [Fact]
    public void ReportsAll_AnyMissing_Throws()
    {
        var ex = Sample("alpha beta");

        Assert.Throws<AssertionFailedException>(() => ex.ReportsAll(["alpha", "missing"]));
    }

    /// <summary>Verifies ReportsExactly passes when the message matches exactly.</summary>
    [Fact]
    public void ReportsExactly_Equal_Passes()
    {
        var ex = Sample("exact text");

        var result = ex.ReportsExactly("exact text");

        Assert.Same(ex, result);
    }

    /// <summary>Verifies ReportsExactly throws when the message differs.</summary>
    [Fact]
    public void ReportsExactly_Differs_Throws()
    {
        var ex = Sample("close but not exact");

        Assert.Throws<AssertionFailedException>(() => ex.ReportsExactly("close"));
    }

    /// <summary>Verifies ReportsAsync passes when the awaited exception message contains the text.</summary>
    [Fact]
    public async Task ReportsAsync_Contains_Passes()
    {
        var task = Task.FromResult(Sample("async fox"));

        var result = await task.ReportsAsync("fox");

        Assert.Equal("async fox", result.Message);
    }

    /// <summary>Verifies ReportsAsync throws when the awaited exception message lacks the text.</summary>
    [Fact]
    public async Task ReportsAsync_DoesNotContain_Throws()
    {
        var task = Task.FromResult(Sample("hello"));

        await Assert.ThrowsAsync<AssertionFailedException>(async () => await task.ReportsAsync("missing"));
    }

    /// <summary>Verifies ReportsAllAsync passes when all texts are present.</summary>
    [Fact]
    public async Task ReportsAllAsync_AllPresent_Passes()
    {
        var task = Task.FromResult(Sample("alpha beta gamma"));

        var result = await task.ReportsAllAsync(["alpha", "gamma"]);

        Assert.Equal("alpha beta gamma", result.Message);
    }

    /// <summary>Verifies ReportsAllAsync throws when any text is missing.</summary>
    [Fact]
    public async Task ReportsAllAsync_AnyMissing_Throws()
    {
        var task = Task.FromResult(Sample("alpha beta"));

        await Assert.ThrowsAsync<AssertionFailedException>(async () =>
            await task.ReportsAllAsync(["alpha", "missing"])
        );
    }

    /// <summary>Verifies ReportsExactlyAsync passes when the messages match exactly.</summary>
    [Fact]
    public async Task ReportsExactlyAsync_Equal_Passes()
    {
        var task = Task.FromResult(Sample("exact"));

        var result = await task.ReportsExactlyAsync("exact");

        Assert.Equal("exact", result.Message);
    }

    /// <summary>Verifies ReportsExactlyAsync throws when the messages differ.</summary>
    [Fact]
    public async Task ReportsExactlyAsync_Differs_Throws()
    {
        var task = Task.FromResult(Sample("close"));

        await Assert.ThrowsAsync<AssertionFailedException>(async () => await task.ReportsExactlyAsync("exact"));
    }
}
