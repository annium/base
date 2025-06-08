using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Reactive.Tests.Operators;

/// <summary>
/// Tests for the CatchAsync operator in reactive extensions.
/// </summary>
public class CatchAsyncTest
{
    /// <summary>
    /// Tests that the CatchAsync operator correctly handles exceptions asynchronously
    /// and continues the observable sequence with the provided fallback.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task CatchAsync_WorksCorrectly()
    {
        // arrange
        var log = new TestLog<string>();
        var tcs = new TaskCompletionSource();
        using var observable = Observable
            .Range(1, 5)
            .Select(x =>
            {
                if (x == 3)
                    throw new InvalidOperationException("3");

                lock (log)
                    log.Add($"add: {x}");

                return x;
            })
            .CatchAsync(
                async (InvalidOperationException e) =>
                {
                    await Task.Delay(10);
                    lock (log)
                        log.Add($"err: {e.Message}");

                    return Observable.Empty<int>();
                }
            )
            .Subscribe(
                x =>
                {
                    lock (log)
                        log.Add($"sub: {x}");
                },
                () =>
                {
                    lock (log)
                        log.Add("done");
                    tcs.SetResult();
                }
            );

        await tcs.Task;

        log.Has(6);
        log[4].Is("err: 3");
        log[5].Is("done");
    }
}
