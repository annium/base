using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Reactive.Tests.Operators;

/// <summary>
/// Tests for the SelectParallelAsync operator in reactive extensions.
/// </summary>
public class SelectParallelAsyncTest : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SelectParallelAsyncTest"/> class.
    /// </summary>
    /// <param name="outputHelper">The test output helper for logging test results.</param>
    public SelectParallelAsyncTest(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        this.RegisterTestLogs();
    }

    /// <summary>
    /// Tests that the SelectParallelAsync operator transforms elements in parallel,
    /// allowing concurrent execution of async transformations.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task SelectParallelAsync_WorksCorrectly()
    {
        // arrange
        var log = Get<TestLog<string>>();
        var tcs = new TaskCompletionSource();
        using var observable = Observable
            .Range(1, 5)
            .SelectParallelAsync(async x =>
            {
                log.Add($"start: {x}");
                await Task.Delay(100);
                log.Add($"end: {x}");
                return x;
            })
            .Subscribe(_ => { }, tcs.SetResult);

        await Bounded.AwaitAsync(tcs.Task);

        log.Has(10);
        var starts = log.Select((x, i) => (x, i)).Where(x => x.x.StartsWith("start:")).Select(x => x.i).ToArray();
        var ends = log.Select((x, i) => (x, i)).Where(x => x.x.StartsWith("end:")).Select(x => x.i).ToArray();

        // at least one start/end pair will have sequential position in log
        starts.Any(x => starts.Contains(x - 1)).IsTrue();
        ends.Any(x => ends.Contains(x - 1)).IsTrue();
    }

    /// <summary>
    /// The selectors run in parallel; the notifications they produce do not. Rx observers are written
    /// against a grammar that promises OnNext calls never overlap, so almost none of them - including the
    /// stock operators one would compose this with - are safe against being re-entered from another
    /// thread. What "parallel" names here is the work, not the delivery.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task SelectParallelAsync_DeliversOneNotificationAtATime()
    {
        // arrange
        var tcs = new TaskCompletionSource();
        var selectorsInFlight = 0;
        var selectorsOverlapped = false;
        var observersInFlight = 0;
        var observersOverlapped = false;

        // act
        using var observable = Observable
            .Range(1, 20)
            .SelectParallelAsync(async x =>
            {
                if (Interlocked.Increment(ref selectorsInFlight) > 1)
                    Volatile.Write(ref selectorsOverlapped, true);
                await Task.Delay(50);
                Interlocked.Decrement(ref selectorsInFlight);

                return x;
            })
            .Subscribe(
                _ =>
                {
                    if (Interlocked.Increment(ref observersInFlight) > 1)
                        Volatile.Write(ref observersOverlapped, true);
                    Thread.Sleep(20);
                    Interlocked.Decrement(ref observersInFlight);
                },
                tcs.SetResult
            );

        await Bounded.AwaitAsync(tcs.Task);

        // assert
        Volatile.Read(ref observersOverlapped).IsFalse("the observer must never be called from two threads at once");
        Volatile
            .Read(ref selectorsOverlapped)
            .IsTrue("the selectors must still run in parallel - serializing them would not be a fix");
    }
}
