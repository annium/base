using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing;
using Xunit;

namespace Annium.Execution.Background.Tests;

/// <summary>
/// Tests for the concurrent background executor implementation
/// </summary>
public class ConcurrentBackgroundExecutorTests : BackgroundExecutorTestBase
{
    public ConcurrentBackgroundExecutorTests(ITestOutputHelper outputHelper)
        : base(x => Executor.Concurrent<ConcurrentBackgroundExecutorTests>(x), outputHelper) { }

    /// <summary>
    /// Tests that the concurrent executor processes tasks correctly with potential out-of-order execution
    /// </summary>
    /// <returns>A task representing the test operation</returns>
    [Fact]
    public async Task Works()
    {
        this.Trace("start");

        // arrange
        var size = Environment.ProcessorCount * 2;

        // act
        var result = await Works_Base(size);

        // assert — every scheduled item ran exactly once (order-independent; asserting out-of-order
        // execution would be a parallel-interleaving signal that spuriously fails on single-core CI)
        var sequence = Enumerable.Range(0, 2 * size).ToArray();
        result.OrderBy(x => x).ToArray().IsEqual(sequence);

        this.Trace("done");
    }

    /// <summary>
    /// Tests the executor's availability state throughout its lifecycle
    /// </summary>
    /// <returns>A task representing the test operation</returns>
    [Fact]
    public async Task Availability()
    {
        this.Trace("start");

        await Availability_Base();

        this.Trace("done");
    }

    /// <summary>
    /// Tests that the executor handles task failures gracefully
    /// </summary>
    /// <returns>A task representing the test operation</returns>
    [Fact]
    public async Task HandlesFailure()
    {
        this.Trace("start");

        await HandlesFailure_Base();

        this.Trace("done");
    }

    /// <summary>
    /// Tests scheduling of synchronous action tasks
    /// </summary>
    /// <returns>A task representing the test operation</returns>
    [Fact]
    public async Task Schedule_SyncAction()
    {
        this.Trace("start");

        await Schedule_SyncAction_Base();

        this.Trace("done");
    }

    /// <summary>
    /// Tests scheduling of synchronous action tasks with cancellation support
    /// </summary>
    /// <returns>A task representing the test operation</returns>
    [Fact]
    public async Task Schedule_SyncCancellableAction()
    {
        this.Trace("start");

        await Schedule_SyncCancellableAction_Base();

        this.Trace("done");
    }

    /// <summary>
    /// Tests scheduling of asynchronous action tasks
    /// </summary>
    /// <returns>A task representing the test operation</returns>
    [Fact]
    public async Task Schedule_AsyncAction()
    {
        this.Trace("start");

        await Schedule_AsyncAction_Base();

        this.Trace("done");
    }

    /// <summary>
    /// Tests scheduling of asynchronous action tasks with cancellation support
    /// </summary>
    /// <returns>A task representing the test operation</returns>
    [Fact]
    public async Task Schedule_AsyncCancellableAction()
    {
        this.Trace("start");

        await Schedule_AsyncCancellableAction_Base();

        this.Trace("done");
    }

    /// <summary>
    /// Verifies that an exception thrown by a scheduled task is surfaced through the logger
    /// (regression guard for the T3 fire-and-forget sweep).
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task ExceptionInTask_LogsError()
    {
        this.Trace("start");

        await ExceptionInTask_LogsError_Base();

        this.Trace("done");
    }

    // -------------------------------------------------------------------------
    // Never-started disposal drain
    // -------------------------------------------------------------------------

    /// <summary>
    /// Disposing a never-started executor with no queued tasks completes cleanly.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task DisposeWithoutStart_NoTasks_CompletesCleanly()
    {
        this.Trace("start");

        await DisposeWithoutStart_NoTasks_CompletesCleanly_Base();

        this.Trace("done");
    }

    /// <summary>
    /// Disposing a never-started executor drains and runs all previously queued tasks.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task DisposeWithoutStart_WithQueuedTasks_RunsAllTasks()
    {
        this.Trace("start");

        await DisposeWithoutStart_WithQueuedTasks_RunsAllTasks_Base();

        this.Trace("done");
    }

    /// <summary>
    /// Disposing a never-started executor whose queued task throws completes without hanging.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task DisposeWithoutStart_ThrowingTask_CompletesWithoutHanging()
    {
        this.Trace("start");

        await DisposeWithoutStart_ThrowingTask_CompletesWithoutHanging_Base();

        this.Trace("done");
    }

    // -------------------------------------------------------------------------
    // Double-start / start-after-dispose guard
    // -------------------------------------------------------------------------

    /// <summary>
    /// Calling Start twice throws InvalidOperationException.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task Start_CalledTwice_ThrowsInvalidOperation()
    {
        this.Trace("start");

        await Start_CalledTwice_ThrowsInvalidOperation_Base();

        this.Trace("done");
    }

    /// <summary>
    /// Calling Start after DisposeAsync throws InvalidOperationException.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task Start_AfterDispose_ThrowsInvalidOperation()
    {
        this.Trace("start");

        await Start_AfterDispose_ThrowsInvalidOperation_Base();

        this.Trace("done");
    }

    // -------------------------------------------------------------------------
    // Double-dispose idempotency
    // -------------------------------------------------------------------------

    /// <summary>
    /// Calling DisposeAsync a second time completes cleanly (State.Disposed early-return guard).
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task DoubleDispose_CompletesCleanly()
    {
        this.Trace("start");

        await DoubleDispose_CompletesCleanly_Base();

        this.Trace("done");
    }

    // -------------------------------------------------------------------------
    // External-token cancellation triggers Stop
    // -------------------------------------------------------------------------

    /// <summary>
    /// Cancelling the token passed to Start stops the executor and makes it unavailable.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task ExternalTokenCancel_StopsExecutor()
    {
        this.Trace("start");

        await ExternalTokenCancel_StopsExecutor_Base();

        this.Trace("done");
    }

    // -------------------------------------------------------------------------
    // Graceful drain on external-token cancellation
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tasks queued before external-token cancellation are still drained and run after Stop().
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task ExternalTokenCancel_DrainsQueuedTasks()
    {
        this.Trace("start");

        await ExternalTokenCancel_DrainsQueuedTasks_Base();

        this.Trace("done");
    }

    // -------------------------------------------------------------------------
    // Parallelism cap
    // -------------------------------------------------------------------------

    /// <summary>
    /// The concurrent executor with parallelism=2 never runs more than 2 tasks simultaneously.
    /// Uses a SemaphoreSlim rendezvous so the test is deterministic and non-flaky.
    /// </summary>
    /// <returns>A task that represents the asynchronous test.</returns>
    [Fact]
    public async Task Concurrent_ParallelismCap_NeverExceedsLimit()
    {
        this.Trace("start");

        const int limit = 2;
        const int taskCount = 6;
        var ct = TestContext.Current.CancellationToken;

        await using var executor = Executor.Concurrent<ConcurrentBackgroundExecutorTests>(Get<ILogger>(), limit);
        executor.Start(ct);

        // Gate: all tasks wait on this before completing, so we can observe the high-water mark.
        // RunContinuationsAsynchronously so SetResult does not run continuations inline on the
        // test thread and block the semaphore drain below.
        var releaseGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        // Rendezvous: counts how many tasks have entered the gate (reached "inside")
        var enteredGate = new SemaphoreSlim(0, taskCount);

        var currentCount = 0;
        var highWaterMark = 0;

        for (var i = 0; i < taskCount; i++)
        {
            executor.Schedule(async () =>
            {
                // increment concurrent-count and update high-water mark
                var current = Interlocked.Increment(ref currentCount);
                int prior;
                do
                {
                    prior = Volatile.Read(ref highWaterMark);
                    if (current <= prior)
                        break;
                } while (Interlocked.CompareExchange(ref highWaterMark, current, prior) != prior);

                // signal entry, then wait for the test thread to release us
                enteredGate.Release();

                // VSTHRD003: releaseGate.Task is this test's own drain gate, not foreign work
#pragma warning disable VSTHRD003
                await releaseGate.Task.ConfigureAwait(false);
#pragma warning restore VSTHRD003

                Interlocked.Decrement(ref currentCount);
            });
        }

        // wait until exactly `limit` tasks have entered (parallelism cap blocks the rest)
        for (var i = 0; i < limit; i++)
            await enteredGate.WaitAsync(ct);

        // release all waiting tasks
        releaseGate.SetResult();

        await executor.DisposeAsync();

        this.Trace("assert high-water mark");
        highWaterMark.IsLessOrEqual(limit);

        this.Trace("done");
    }
}
