using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Jobs.Tests;

/// <summary>
/// Tests for the scheduler's own loop, as opposed to the interval parsing it delegates to. A scheduled job
/// is fire-and-forget from the caller's side, so the loop surviving a failing run is the only thing that
/// keeps the schedule alive.
/// </summary>
public class SchedulerTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SchedulerTests"/> class.
    /// </summary>
    /// <param name="outputHelper">xUnit test output helper the test host logs through.</param>
    public SchedulerTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        Register(container =>
        {
            container.AddTime().WithRealTime().SetDefault();
            container.AddScheduler();
        });
    }

    /// <summary>
    /// The schedule keeps firing after a run throws. A job that stops for good on its first bad run is a
    /// silent outage: nothing surfaces to the caller, who is holding only a cancellation handle.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Schedule_HandlerThrows_KeepsRunning()
    {
        // arrange
        var runs = 0;
        var scheduler = Get<IScheduler>();

        // act - every run throws, so surviving one failure is not enough; the loop has to keep going
        using var handle = scheduler.Schedule(
            () =>
            {
                Interlocked.Increment(ref runs);

                throw new InvalidOperationException("scheduled work failed");
            },
            Interval.Secondly
        );

        // assert
        await WaitUntilAsync(() => Volatile.Read(ref runs) >= 3);
    }

    /// <summary>
    /// Disposing the handle stops the schedule.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Schedule_Disposed_StopsRunning()
    {
        // arrange
        var runs = 0;
        var scheduler = Get<IScheduler>();
        var handle = scheduler.Schedule(
            () =>
            {
                Interlocked.Increment(ref runs);

                return Task.CompletedTask;
            },
            Interval.Secondly
        );
        await WaitUntilAsync(() => Volatile.Read(ref runs) >= 1);

        // act - the handle is a plain IDisposable cancel token, not async work
        // VSTHRD103: Dispose here only cancels a token source; there is no async counterpart
#pragma warning disable VSTHRD103
        handle.Dispose();
#pragma warning restore VSTHRD103
        var afterDispose = Volatile.Read(ref runs);
        await Task.Delay(TimeSpan.FromSeconds(2), TestContext.Current.CancellationToken);

        // assert - at most the run already in flight when the handle was disposed
        (Volatile.Read(ref runs) - afterDispose <= 1).IsTrue("the schedule must stop once disposed");
    }

    /// <summary>
    /// Polls until the condition holds, failing the test if it does not within 15 seconds.
    /// </summary>
    /// <param name="condition">The condition to wait for.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        var sw = Stopwatch.StartNew();
        while (!condition() && sw.Elapsed < TimeSpan.FromSeconds(15))
            await Task.Delay(50, TestContext.Current.CancellationToken);

        condition().IsTrue("condition was not met within 15s");
    }
}
