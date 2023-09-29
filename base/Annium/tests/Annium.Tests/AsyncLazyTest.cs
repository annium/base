using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Tests;

public class AsyncLazyTest
{
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
}