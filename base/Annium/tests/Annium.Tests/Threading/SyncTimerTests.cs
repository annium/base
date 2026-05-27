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
        var state = new TimerTestHelpers.State();
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
        var state = new TimerTestHelpers.State();
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
        var state = new TimerTestHelpers.State();
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
        var state = new TimerTestHelpers.State();
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
    /// When the in-flight synchronous callback runs longer than <c>DisposeWaitBudget</c>, the
    /// <see cref="System.Threading.Timer.Dispose(WaitHandle)"/> drain times out: dispose returns without
    /// throwing and a warning is logged. The wait handle is intentionally leaked so the still-blocked
    /// ThreadPool thread can eventually unblock and return without raising
    /// <see cref="ObjectDisposedException"/>.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Dispose_GateDrainTimesOut_LogsWarningAndDoesNotThrow()
    {
        // arrange — handler that blocks on its OWN gate past the 5s DisposeWaitBudget.
        // The handler MUST NOT honour the xunit runner CT (otherwise early cancellation would let the
        // handler return before the drain times out and the warn-log branch would never run).
        // Pattern:
        //   * handler waits on a ManualResetEventSlim using its own CTS — independent of the runner CT
        //   * handler signals a TCS in its finally so the test can deterministically wait for cleanup
        //   * test cancels the handler's gate AFTER Dispose returns (so the drain budget elapses first)
        //     then awaits the TCS to ensure the handler has fully returned
        using var handlerCts = new CancellationTokenSource();
        var handlerEntered = new ManualResetEventSlim(false);
        var handlerExited = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var blockGate = new ManualResetEventSlim(false);

        var timer = Timers.Sync(
            () =>
            {
                handlerEntered.Set();
                try
                {
                    blockGate.Wait(handlerCts.Token);
                }
                catch (OperationCanceledException) { }
                finally
                {
                    handlerExited.TrySetResult(true);
                }
            },
            0,
            10,
            Logger
        );

        // wait until the handler is in-flight so Dispose has work to drain
        handlerEntered.Wait(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // act — dispose; the inner drain will time out after ~5s and the warn-log path runs
        await timer.DisposeAsync();

        // assert — drain-timeout warning was logged with the expected template
        Logs.Any(l => l.Level == LogLevel.Warn && l.MessageTemplate.Contains("Timer drain exceeded")).IsTrue();

        // cleanup — release the handler and wait for it to finish so the blocked ThreadPool thread
        // is not left running after the test ends
        await handlerCts.CancelAsync();
        await handlerExited.Task.WaitAsync(TimeSpan.FromSeconds(2), TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Validates the timer state captured during a test run by delegating to the shared
    /// <see cref="TimerTestHelpers.EnsureValidAsync"/> helper, which asserts that callbacks
    /// were always entered and exited in matched pairs.
    /// </summary>
    /// <param name="state">The accumulated timer state to validate.</param>
    /// <returns>A task that completes when the validation assertion finishes.</returns>
    private static Task EnsureValid(TimerTestHelpers.State state) => TimerTestHelpers.EnsureValidAsync(state);

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
