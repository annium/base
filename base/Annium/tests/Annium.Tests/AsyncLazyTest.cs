using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Tests;

/// <summary>
/// Contains unit tests for <see cref="AsyncLazy{T}"/> to verify lazy initialization behavior.
/// </summary>
public class AsyncLazyTest
{
    /// <summary>
    /// Verifies that the synchronous factory works as expected.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SyncFactory_Works()
    {
        // arrange
        var lazy = new AsyncLazy<int>(() => 10);

        // act
        var value = await lazy;

        // assert
        value.Is(10);
    }

    /// <summary>
    /// Verifies that the synchronous factory works correctly when accessed concurrently.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SyncFactory_Concurrent_Works()
    {
        // arrange
        var lazy = new AsyncLazy<object>(() => new object());

        // act
        var values = await Task.WhenAll(
            Task.Run(async () => await lazy),
            Task.Run(async () => await lazy),
            Task.Run(async () => await lazy),
            Task.Run(async () => await lazy),
            Task.Run(async () => await lazy),
            Task.Run(async () => await lazy),
            Task.Run(async () => await lazy),
            Task.Run(async () => await lazy),
            Task.Run(async () => await lazy),
            Task.Run(async () => await lazy)
        );

        // assert
        var subject = values[0];
        foreach (var value in values)
            value.Is(subject);
    }

    /// <summary>
    /// Verifies that the asynchronous factory works as expected.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task AsyncFactory_Works()
    {
        // arrange
        var lazy = new AsyncLazy<int>(async () =>
        {
            await Task.Delay(5);
            return 10;
        });

        // act
        var value = await lazy;

        // assert
        value.Is(10);
    }

    /// <summary>
    /// Verifies that the asynchronous factory works correctly when accessed concurrently.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task AsyncFactory_Concurrent_Works()
    {
        // arrange
        var lazy = new AsyncLazy<object>(async () =>
        {
            await Task.Delay(25);
            return new object();
        });

        // act
        var values = await Task.WhenAll(
            Task.Run(async () => await lazy),
            Task.Run(async () => await lazy),
            Task.Run(async () => await lazy),
            Task.Run(async () => await lazy),
            Task.Run(async () => await lazy),
            Task.Run(async () => await lazy),
            Task.Run(async () => await lazy),
            Task.Run(async () => await lazy),
            Task.Run(async () => await lazy),
            Task.Run(async () => await lazy)
        );

        // assert
        var subject = values[0];
        foreach (var value in values)
            value.Is(subject);
    }

    /// <summary>
    /// Verifies that <c>GetValueAsync</c> returns the lazily-produced value (T8 — replaces the
    /// removed <c>Value</c> sync trapdoor as the explicit accessor).
    /// </summary>
    [Fact]
    public async Task GetValueAsync_SyncFactory_ReturnsValue()
    {
        // arrange
        var lazy = new AsyncLazy<int>(() => 42);

        // act
        var value = await lazy.GetValueAsync(TestContext.Current.CancellationToken);

        // assert
        value.Is(42);
    }

    /// <summary>
    /// Verifies that <c>GetValueAsync</c> works for an async factory.
    /// </summary>
    [Fact]
    public async Task GetValueAsync_AsyncFactory_ReturnsValue()
    {
        // arrange
        var lazy = new AsyncLazy<int>(async () =>
        {
            await Task.Delay(5);
            return 99;
        });

        // act
        var value = await lazy.GetValueAsync(TestContext.Current.CancellationToken);

        // assert
        value.Is(99);
    }

    /// <summary>
    /// Verifies the <c>Value</c> property has been removed (T8 — sync trapdoor closed).
    /// </summary>
    [Fact]
    public void Value_PropertyDoesNotExist()
    {
        var prop = typeof(AsyncLazy<int>).GetProperty("Value");
        prop.Is(null);
    }
}
