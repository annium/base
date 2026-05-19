using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing;
using Annium.Threading;
using Annium.Threading.Tasks;
using Xunit;

namespace Annium.Tests.Threading;

/// <summary>
/// Contains unit tests for the SyncTimer class.
/// </summary>
public class SyncTimerTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the SyncTimerTests class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public SyncTimerTests(ITestOutputHelper outputHelper)
        : base(outputHelper) { }

    /// <summary>
    /// Verifies that stateful timer works correctly with overlapping executions.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Stateful_Overlapping()
    {
        this.Trace("start");

        // arrange
        var state = new State();
        using var timer = Timers.Sync(
            state,
            static state =>
            {
                state.Push();
                Thread.Sleep(3);
                state.Push();
            },
            0,
            1,
            Logger
        );

        // act
        await Task.Delay(50, TestContext.Current.CancellationToken);
        timer.Change(Timeout.Infinite, Timeout.Infinite);

        // assert
        this.Trace("ensure state is valid");
        await EnsureValid(state);

        this.Trace("done");
    }

    /// <summary>
    /// Verifies that stateful timer works correctly with concurrent starts.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Stateful_ConcurrentStart()
    {
        this.Trace("start");

        // arrange
        var state = new State();
        using var timer = Timers.Sync(
            state,
            static state =>
            {
                state.Push();
                Thread.Sleep(3);
                state.Push();
            },
            0,
            2,
            Logger
        );
        timer.Change(0, 1);

        // act
        await Task.Delay(50, TestContext.Current.CancellationToken);
        timer.Change(Timeout.Infinite, Timeout.Infinite);

        // assert
        this.Trace("ensure state is valid");
        await EnsureValid(state);

        this.Trace("done");
    }

    /// <summary>
    /// Verifies that stateless timer works correctly with overlapping executions.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Stateless_Overlapping()
    {
        this.Trace("start");

        // arrange
        var state = new State();
        using var timer = Timers.Sync(
            () =>
            {
                state.Push();
                Thread.Sleep(3);
                state.Push();
            },
            0,
            1,
            Logger
        );

        // act
        await Task.Delay(50, TestContext.Current.CancellationToken);
        timer.Change(Timeout.Infinite, Timeout.Infinite);

        // assert
        this.Trace("ensure state is valid");
        await EnsureValid(state);

        this.Trace("done");
    }

    /// <summary>
    /// Verifies that stateless timer works correctly with concurrent starts.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Stateless_ConcurrentStart()
    {
        this.Trace("start");

        // arrange
        var state = new State();
        using var timer = Timers.Sync(
            () =>
            {
                state.Push();
                Thread.Sleep(3);
                state.Push();
            },
            0,
            2,
            Logger
        );
        timer.Change(0, 1);

        // act
        await Task.Delay(50, TestContext.Current.CancellationToken);
        timer.Change(Timeout.Infinite, Timeout.Infinite);

        // assert
        this.Trace("ensure state is valid");
        await EnsureValid(state);

        this.Trace("done");
    }

    /// <summary>
    /// Ensures that the state is valid by checking the sequence of numbers.
    /// </summary>
    /// <param name="state">The state to validate.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task EnsureValid(State state)
    {
        // Bounded wait until timers complete (step is executed to end). Replaces the previous
        // unbounded `do { await Task.Delay(5); } while (count % 2 > 0)` loop that could hang the
        // test runner indefinitely if a timer regression caused the count to stop advancing.
        await Wait.UntilAsync(() => state.Data.Count % 2 == 0, ms: 5000);

        // Snapshot under ConcurrentQueue.ToArray() — safe against a queued ThreadPool callback
        // that may still call Push() after the underlying timer was stopped via Change(Infinite, Infinite)
        // but before its queued callbacks have drained.
        var snapshot = state.Data.ToArray();
        var expectedData = Enumerable.Range(0, snapshot.Length).ToArray();
        snapshot.SequenceEqual(expectedData).IsTrue();
    }

    /// <summary>
    /// Verifies that a stateless SyncTimer continues firing after the handler throws an exception.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task HandlerThrows_SyncTimer_ContinuesFiring()
    {
        this.Trace("start");

        // arrange
        var calls = 0;
        var pushes = new ConcurrentQueue<int>();
        using var timer = Timers.Sync(
            () =>
            {
                var n = Interlocked.Increment(ref calls);
                if (n == 1)
                    throw new InvalidOperationException("boom");
                pushes.Enqueue(n);
            },
            0,
            20,
            Logger
        );

        // act — wait for at least 2 successful (non-throwing) invocations
        await Wait.UntilAsync(() => pushes.Count >= 2, ms: 2000);
        timer.Change(Timeout.Infinite, Timeout.Infinite);

        // assert — timer did not stall after the exception
        this.Trace("assert pushes");
        (pushes.Count >= 2).IsTrue();

        this.Trace("done");
    }

    /// <summary>
    /// Verifies that a stateful SyncTimer continues firing after the handler throws an exception.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task HandlerThrows_SyncTimerStateful_ContinuesFiring()
    {
        this.Trace("start");

        // arrange
        var statefulState = new ThrowState();
        using var timer = Timers.Sync(
            statefulState,
            static s =>
            {
                var n = Interlocked.Increment(ref s.Calls);
                if (n == 1)
                    throw new InvalidOperationException("boom");
                s.Pushes.Enqueue(n);
            },
            0,
            20,
            Logger
        );

        // act — wait for at least 2 successful (non-throwing) invocations
        await Wait.UntilAsync(() => statefulState.Pushes.Count >= 2, ms: 2000);
        timer.Change(Timeout.Infinite, Timeout.Infinite);

        // assert — timer did not stall after the exception
        this.Trace("assert pushes");
        (statefulState.Pushes.Count >= 2).IsTrue();

        this.Trace("done");
    }

    /// <summary>
    /// Verifies that disposing a SyncTimer from inside its own handler does not deadlock.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Dispose_Reentrant_FromInsideSyncHandler_DoesNotDeadlock()
    {
        this.Trace("start");

        // arrange
        ISequentialTimer? timer = null;
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        timer = Timers.Sync(
            () =>
            {
                // Capture and dispose re-entrantly on the first invocation only
                if (!tcs.Task.IsCompleted)
                {
                    timer!.Dispose();
                    tcs.TrySetResult(true);
                }
            },
            0,
            50,
            Logger
        );

        // act — bounded wait so a deadlock regression fails the test instead of hanging
        await Wait.UntilAsync(() => tcs.Task.IsCompleted, ms: 2000);

        // assert — the dispose call returned without deadlocking
        this.Trace("assert tcs completed");
        tcs.Task.IsCompleted.IsTrue();

        this.Trace("done");
    }

    /// <summary>
    /// A class that maintains a queue of integers for testing.
    /// </summary>
    private class State
    {
        /// <summary>
        /// Gets the queue of integers. <see cref="ConcurrentQueue{T}"/> is used so iteration via
        /// <c>ToArray</c> is snapshot-safe against races with queued timer callbacks calling
        /// <see cref="Push"/> after <c>timer.Change(Infinite, Infinite)</c>.
        /// </summary>
        public ConcurrentQueue<int> Data { get; } = new();

        /// <summary>
        /// Adds the current count to the queue.
        /// </summary>
        public void Push()
        {
            Data.Enqueue(Data.Count);
        }
    }

    /// <summary>
    /// A class that holds mutable state for the exception-resilience tests.
    /// </summary>
    private class ThrowState
    {
        /// <summary>
        /// Gets or sets the invocation counter.
        /// </summary>
        public int Calls;

        /// <summary>
        /// Gets the queue of successfully recorded invocation numbers.
        /// </summary>
        public ConcurrentQueue<int> Pushes { get; } = new();
    }
}
