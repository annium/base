using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Reactive.Tests.Operators;

public class DoSequentialAsyncTest
{
    [Fact]
    public async Task DoSequentialAsync_WorksCorrectly()
    {
        // arrange
        var log = new TestLog<string>();
        var tcs = new TaskCompletionSource();
        using var observable = Observable
            .Range(1, 100)
            .DoSequentialAsync(async x =>
            {
                log.Add($"start: {x}");
                await Task.Delay(10);
                log.Add($"end: {x}");
            })
            .Subscribe(
                x =>
                {
                    log.Add($"sub: {x}");
                },
                tcs.SetResult
            );

        await tcs.Task;

        log.Has(300);
        var expectedLog = Enumerable
            .Range(1, 100)
            .Select(x => new[] { $"start: {x}", $"end: {x}", $"sub: {x}" })
            .SelectMany(x => x)
            .ToArray();
        log.IsEqual(expectedLog);
    }
}
