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
/// Contains unit tests for the AsyncTimer class.
/// </summary>
public class AsyncTimerTests : TestBase
{
    /// <summary>
    /// Initializes a new instance of the AsyncTimerTests class.
    /// </summary>
    /// <param name="outputHelper">The test output helper.</param>
    public AsyncTimerTests(ITestOutputHelper outputHelper)
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
        using var timer = Timers.Async(
            state,
            static async state =>
            {
                state.Push();
                await Task.Delay(3);
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
        using var timer = Timers.Async(
            state,
            static async state =>
            {
                state.Push();
                await Task.Delay(3);
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
        using var timer = Timers.Async(
            async () =>
            {
                state.Push();
                await Task.Delay(3);
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
        using var timer = Timers.Async(
            async () =>
            {
                state.Push();
                await Task.Delay(3);
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
    /// Verifies that calling DisposeAsync twice is idempotent and does not deadlock (review T5).
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Dispose_IsIdempotent_SecondCallReturnsImmediately()
    {
        var timer = Timers.Async(static () => ValueTask.CompletedTask, 0, 10, Logger);

        await timer.DisposeAsync();
        await timer.DisposeAsync();

        // Reaching here without hang or exception is the assertion.
        true.IsTrue();
    }

    /// <summary>
    /// Verifies that calling Dispose from inside the handler does not deadlock (review T5 — re-entrant dispose).
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Dispose_Reentrant_FromInsideHandler_DoesNotDeadlock()
    {
        ISequentialTimer? timer = null;
        var disposed = false;

        timer = Timers.Async(
            async () =>
            {
                await timer!.DisposeAsync();
                disposed = true;
            },
            0,
            10,
            Logger
        );

        await Wait.UntilAsync(() => disposed, ms: 5000);
        disposed.IsTrue();
    }

    /// <summary>
    /// Verifies that an exception thrown by the handler does not stop subsequent ticks (review T6).
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task HandlerThrows_TimerContinuesFiring()
    {
        var calls = 0;
        var successAfterThrow = false;

        using var timer = Timers.Async(
            () =>
            {
                var n = Interlocked.Increment(ref calls);
                if (n <= 2)
                    throw new InvalidOperationException($"intentional fault on tick {n}");
                successAfterThrow = true;
                return ValueTask.CompletedTask;
            },
            0,
            5,
            Logger
        );

        await Wait.UntilAsync(() => successAfterThrow, ms: 5000);

        successAfterThrow.IsTrue();
        (calls >= 3).IsTrue();
    }

    /// <summary>
    /// When the in-flight async callback runs longer than <c>DisposeWaitBudget</c>, the gate drain
    /// times out: dispose returns without throwing and a warning is logged. The gate is intentionally
    /// leaked so the still-running callback can complete without <see cref="ObjectDisposedException"/>.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task Dispose_GateDrainTimesOut_LogsWarningAndDoesNotThrow()
    {
        // arrange — handler that blocks past the 5s DisposeWaitBudget. The handler must NOT honour
        // the xunit runner CT (otherwise early cancellation would let the handler return before
        // OnDrainCompleted times out, and the warn-log branch would never run). It also must not
        // leak past test end: an `async void` continuation parked on Task.Delay would otherwise
        // occupy a ThreadPool slot for ~5s after the test method returns. The pattern:
        //   * handler awaits Task.Delay on its OWN CTS — independent of the runner CT
        //   * handler signals a TCS in its finally so the test can deterministically wait for it
        //   * test cancels the handler CTS AFTER DisposeAsync returns (so the drain budget elapses
        //     and the warn path is taken), then awaits the TCS to ensure the handler is fully done
        using var handlerCts = new CancellationTokenSource();
        var handlerEntered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var handlerExited = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var timer = Timers.Async(
            async () =>
            {
                handlerEntered.TrySetResult(true);
                try
                {
                    await Task.Delay(7_000, handlerCts.Token);
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

        // wait until the handler is in-flight so DisposeAsync has work to drain
        await handlerEntered.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        // act — dispose; the inner gate drain will time out after ~5s and the warn-log path runs
        await timer.DisposeAsync();

        // assert — drain-timeout warning was logged with the expected template
        Logs.Any(l =>
                l.Level == LogLevel.Warn && l.MessageTemplate.Contains("Timer disposed but in-flight callback exceeded")
            )
            .IsTrue();

        // cleanup — release the handler and wait for it to finish so no async-void continuation
        // leaks past test end. The dispose already happened, so cancelling the handler here is purely
        // about clean shutdown — it does not affect the assertion above.
        await handlerCts.CancelAsync();
        await handlerExited.Task.WaitAsync(TimeSpan.FromSeconds(2), TestContext.Current.CancellationToken);
    }

    private static Task EnsureValid(TimerTestHelpers.State state) => TimerTestHelpers.EnsureValidAsync(state);
}
