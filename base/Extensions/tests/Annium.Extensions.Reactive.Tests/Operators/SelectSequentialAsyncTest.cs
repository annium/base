using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Annium.Extensions.Reactive.Operators;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Reactive.Tests.Operators;

/// <summary>
/// Tests for the SelectSequentialAsync operator in reactive extensions.
/// </summary>
public class SelectSequentialAsyncTest
{
    /// <summary>
    /// Tests that the SelectSequentialAsync operator transforms elements sequentially,
    /// ensuring proper ordering of async transformations.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task SelectSequentialAsync_WorksCorrectly()
    {
        // arrange
        var log = new TestLog<string>();
        var tcs = new TaskCompletionSource();
        using var observable = Observable
            .Range(1, 5)
            .SelectSequentialAsync(async x =>
            {
                lock (log)
                    log.Add($"start: {x}");
                await Task.Delay(10);
                lock (log)
                    log.Add($"end: {x}");
                return x;
            })
            .Subscribe(
                x =>
                {
                    lock (log)
                        log.Add($"sub: {x}");
                },
                tcs.SetResult
            );

        await tcs.Task;

        log.Has(15);
        var expectedLog = Enumerable
            .Range(1, 5)
            .Select(x => new[] { $"start: {x}", $"end: {x}", $"sub: {x}" })
            .SelectMany(x => x)
            .ToArray();
        log.IsEqual(expectedLog);
    }
}
