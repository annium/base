using System;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Testing.Tests;

/// <summary>Tests for <see cref="DelegateExtensions"/>.</summary>
public class DelegateExtensionsTests
{
    /// <summary>Throws returns the caught exception when the delegate throws the expected type.</summary>
    [Fact]
    public void Throws_ReturnsCaughtException()
    {
        Action action = () => throw new InvalidOperationException("boom");
        var ex = Wrap.It(action).Throws<InvalidOperationException>();
        Assert.Equal("boom", ex.Message);
    }

    /// <summary>Throws raises an AssertionFailedException when the delegate does not throw at all.</summary>
    [Fact]
    public void Throws_FailsWhenNoException()
    {
        Action action = () => { };
        Assert.Throws<AssertionFailedException>(() => Wrap.It(action).Throws<InvalidOperationException>());
    }

    /// <summary>Throws raises an AssertionFailedException when the delegate throws a different type.</summary>
    [Fact]
    public void Throws_FailsWhenWrongExceptionType()
    {
        Action action = () => throw new ArgumentException();
        Assert.Throws<AssertionFailedException>(() => Wrap.It(action).Throws<InvalidOperationException>());
    }

    /// <summary>ThrowsAsync returns the caught exception when the async delegate throws the expected type.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ThrowsAsync_ReturnsCaughtException()
    {
        var ex = await Wrap
            .It(async () =>
            {
                await Task.Yield();
                throw new InvalidOperationException("async-boom");
            })
            .ThrowsAsync<InvalidOperationException>();

        Assert.Equal("async-boom", ex.Message);
    }

    /// <summary>ThrowsAsync raises an AssertionFailedException when the async delegate does not throw.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task ThrowsAsync_FailsWhenNoException()
    {
        await Assert.ThrowsAsync<AssertionFailedException>(
            async () => await Wrap.It(async () => await Task.Yield()).ThrowsAsync<InvalidOperationException>()
        );
    }
}
