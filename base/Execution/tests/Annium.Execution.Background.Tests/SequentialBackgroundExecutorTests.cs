using System.Linq;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing;
using Xunit;

namespace Annium.Execution.Background.Tests;

/// <summary>
/// Tests for the sequential background executor implementation
/// </summary>
public class SequentialBackgroundExecutorTests : BackgroundExecutorTestBase
{
    public SequentialBackgroundExecutorTests(ITestOutputHelper outputHelper)
        : base(Executor.Sequential<SequentialBackgroundExecutorTests>, outputHelper) { }

    /// <summary>
    /// Tests that the sequential executor processes tasks in order
    /// </summary>
    /// <returns>A task representing the test operation</returns>
    [Fact]
    public async Task Works()
    {
        this.Trace("start");

        // arrange
        var size = 4;

        // act
        var result = await Works_Base(size);

        // assert
        var sequence = Enumerable.Range(0, size).SelectMany(x => new[] { x, x + size }).ToArray();
        result.IsEqual(sequence);

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
    /// Verifies that an exception thrown by a scheduled task is surfaced through the logger.
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
}
