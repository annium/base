using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing;
using Annium.Threading;
using Annium.Threading.Tasks;
using Xunit;

namespace Annium.Tests.Threading;

/// <summary>
/// Contains unit tests for the DebounceTimer class.
/// </summary>
public class DebounceTimerTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the DebounceTimerTests class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public DebounceTimerTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Verifies that stateful debounce timer works correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Stateful()
    {
        this.Trace("start");

        // arrange
        var state = new State();
        using var timer = Timers.Debounce(
            state,
            s =>
            {
                this.Trace("start");
                s.Push();
                this.Trace("done");

                return ValueTask.CompletedTask;
            },
            // Debounce period (80ms) deliberately dominates the worst-case inter-chunk Task.Delay jitter
            // (Task.Delay(1) can stretch to ~15ms under load); the three chunks therefore coalesce into a
            // single fire per bulk regardless of scheduler jitter, keeping the fire count deterministic.
            80,
            Logger
        );

        // act
        this.Trace("schedule");
        for (var i = 0; i < 3; i++)
        {
            this.Trace("bulk start");
            for (var j = 0; j < 3; j++)
            {
                this.Trace("chunk start");
                Parallel.ForEach(Enumerable.Range(0, 5), _ => timer.Request());
                this.Trace("chunk delay");
                await Task.Delay(1, TestContext.Current.CancellationToken);
            }
            // Idle longer than the debounce period so the single coalesced fire lands within this bulk's
            // window, before the next bulk's requests re-arm the timer.
            this.Trace("bulk delay");
            await Task.Delay(200, TestContext.Current.CancellationToken);
        }

        // assert
        this.Trace("ensure state is valid");
        await EnsureValid(state, 3, 5);

        this.Trace("done");
    }

    /// <summary>
    /// Verifies that stateless debounce timer works correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Stateless()
    {
        this.Trace("start");

        // arrange
        var state = new State();
        using var timer = Timers.Debounce(
            () =>
            {
                this.Trace("start");
                state.Push();
                this.Trace("done");

                return ValueTask.CompletedTask;
            },
            // Debounce period (80ms) deliberately dominates the worst-case inter-chunk Task.Delay jitter
            // (Task.Delay(1) can stretch to ~15ms under load); the three chunks therefore coalesce into a
            // single fire per bulk regardless of scheduler jitter, keeping the fire count deterministic.
            80,
            Logger
        );

        // act
        this.Trace("schedule");
        for (var i = 0; i < 3; i++)
        {
            this.Trace("bulk start");
            for (var j = 0; j < 3; j++)
            {
                this.Trace("chunk start");
                Parallel.ForEach(Enumerable.Range(0, 5), _ => timer.Request());
                this.Trace("chunk delay");
                await Task.Delay(1, TestContext.Current.CancellationToken);
            }
            // Idle longer than the debounce period so the single coalesced fire lands within this bulk's
            // window, before the next bulk's requests re-arm the timer.
            this.Trace("bulk delay");
            await Task.Delay(200, TestContext.Current.CancellationToken);
        }

        // assert
        this.Trace("ensure state is valid");
        await EnsureValid(state, 3, 5);

        this.Trace("done");
    }

    /// <summary>
    /// Ensures that the state is valid by checking the sequence of numbers and count.
    /// </summary>
    /// <param name="state">The state to validate.</param>
    /// <param name="min">The minimum expected count.</param>
    /// <param name="max">The maximum expected count.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task EnsureValid(State state, int min, int max)
    {
        this.Trace("await for {min}-{max} entries in state", min, max);
        await Expect.ToAsync(() =>
        {
            state.Data.Count.IsGreaterOrEqual(min);
            state.Data.Count.IsLessOrEqual(max);
        });

        this.Trace("verify state integrity");
        var expectedData = Enumerable.Range(0, state.Data.Count).ToArray();
        state.Data.SequenceEqual(expectedData).IsTrue();

        this.Trace("done");
    }

    /// <summary>
    /// Verifies that Request() called after DisposeAsync() does not throw (review T7 — post-dispose guard).
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Request_AfterDispose_DoesNotThrow()
    {
        var timer = Timers.Debounce(static () => ValueTask.CompletedTask, 10, Logger);
        await timer.DisposeAsync();

        // Must not throw — the IsDisposed guard + ObjectDisposedException catch in Request() absorbs the race.
        timer.Request();
        timer.Request();
    }

    /// <summary>
    /// Verifies that Request() racing concurrently with DisposeAsync() neither hangs nor surfaces an unhandled
    /// exception (review T7 — race window between IsDisposed check and timer.Change).
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Request_RaceWithDispose_NeitherHangsNorThrows()
    {
        var timer = Timers.Debounce(static () => ValueTask.CompletedTask, 10, Logger);
        var ct = TestContext.Current.CancellationToken;

        var requester = Task.Run(
            () =>
            {
                for (var i = 0; i < 1000; i++)
                    timer.Request();
            },
            ct
        );

        await timer.DisposeAsync();
        await requester;
    }

    /// <summary>
    /// A class that maintains a queue of integers for testing.
    /// </summary>
    private class State
    {
        /// <summary>
        /// Gets the queue of integers.
        /// </summary>
        public Queue<int> Data { get; } = new();

        /// <summary>
        /// Adds the current count to the queue.
        /// </summary>
        public void Push()
        {
            Data.Enqueue(Data.Count);
        }
    }

    /// <summary>
    /// Verifies that Change(TimeSpan) updates the period; the new TimeSpan overload must be
    /// observable when subsequent Request() events fire at the new interval.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Change_TimeSpan_UpdatesPeriod()
    {
        this.Trace("start");

        // arrange — start with a long period (1s) so the first Request would not fire within the test window.
        var state = new State();
        using var timer = Timers.Debounce(
            state,
            s =>
            {
                s.Push();
                return ValueTask.CompletedTask;
            },
            1_000,
            Logger
        );

        // act — change to 10ms before requesting.
        timer.Change(TimeSpan.FromMilliseconds(10));
        timer.Request();
        await Wait.UntilAsync(() => state.Data.Count == 1, 500);

        // assert
        state.Data.Count.Is(1);
    }

    /// <summary>
    /// Verifies that Change(TimeSpan) throws OverflowException for TimeSpans whose total milliseconds
    /// exceed int.MaxValue. Loud failure is preferable to silent overflow into a negative period.
    /// </summary>
    [Fact]
    public void Change_TimeSpan_Overflow_Throws()
    {
        // arrange
        var state = new State();
        using var timer = Timers.Debounce(
            state,
            s =>
            {
                s.Push();
                return ValueTask.CompletedTask;
            },
            20,
            Logger
        );

        // act / assert — TimeSpan.MaxValue overflows int.MaxValue ms.
        Wrap.It(() => timer.Change(TimeSpan.MaxValue)).Throws<OverflowException>();
    }
}
