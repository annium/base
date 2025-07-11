using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Reactive.Tests.Operators;

/// <summary>
/// Tests for the SubscribeAsync operator in reactive extensions.
/// </summary>
public class SubscribeAsyncTest
{
    /// <summary>
    /// Tests that the SubscribeAsync operator correctly handles errors asynchronously
    /// when subscribing to an observable sequence.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task SubscribeAsync_OnErrorWorksCorrectly()
    {
        // arrange
        var log = new TestLog<string>();
        var tcs = new TaskCompletionSource();
        await using var observable = Observable
            .Range(1, 5)
            .Select(x =>
            {
                if (x == 3)
                    throw new InvalidOperationException("3");

                lock (log)
                    log.Add($"add: {x}");

                return x;
            })
            .SubscribeAsync(async e =>
            {
                await Task.Delay(10);
                lock (log)
                    log.Add($"err: {e.Message}");
                await Task.Delay(10);
                tcs.SetResult();
            });

        await tcs.Task;

        log.Has(3);
        log[2].Is("err: 3");
    }

    /// <summary>
    /// Tests that the SubscribeAsync operator correctly handles completion asynchronously
    /// when subscribing to an observable sequence.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task SubscribeAsync_OnCompletedWorksCorrectly()
    {
        // arrange
        var log = new TestLog<string>();
        var tcs = new TaskCompletionSource();
        await using var observable = Observable
            .Range(1, 5)
            .Select(x =>
            {
                lock (log)
                    log.Add($"add: {x}");

                return x;
            })
            .SubscribeAsync(async () =>
            {
                await Task.Delay(10);
                lock (log)
                    log.Add("done");
                tcs.SetResult();
            });

        await tcs.Task;

        log.Has(6);
        log[5].Is("done");
    }
}
