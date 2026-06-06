using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Logging.Shared;
using Annium.Testing;
using Xunit;

namespace Annium.Execution.Background.Tests;

// Note: _executor is the shared fixture executor managed by the base.  The _Base helpers below
// that need a *fresh* executor create their own local one and dispose it themselves, so they never
// touch _executor.  This keeps teardown logic clean.

/// <summary>
/// Base class for testing background executor implementations
/// </summary>
public abstract class BackgroundExecutorTestBase : TestBase
{
    /// <summary>
    /// Factory for the executor under test (resolved against the logger materialized in <see cref="InitializeAsync"/>).
    /// </summary>
    private readonly Func<ILogger, IExecutor> _getExecutor;

    /// <summary>
    /// The executor instance being tested. Materialized in <see cref="InitializeAsync"/> once the provider is built.
    /// </summary>
    // assigned in InitializeAsync (called by the xunit lifecycle) before any test method runs
    private IExecutor _executor = null!;

    protected BackgroundExecutorTestBase(Func<ILogger, IExecutor> getExecutor, ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        _getExecutor = getExecutor;
    }

    /// <summary>
    /// Initializes the test by calling the base setup and constructing the executor under test
    /// using the factory supplied at construction time.
    /// </summary>
    /// <returns>A task that represents the asynchronous initialization.</returns>
    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        _executor = _getExecutor(Get<ILogger>());
    }

    /// <summary>
    /// Disposes the executor under test as a teardown safety net so that a test which throws before its
    /// own explicit <c>await _executor.DisposeAsync()</c> still tears down the background loop and the
    /// executor's <c>CancellationTokenSource</c>. The executor's <c>DisposeAsync</c> is idempotent,
    /// so disposing again here is harmless when the test already disposed it.
    /// </summary>
    /// <returns>A task that represents the asynchronous teardown.</returns>
    public override async ValueTask DisposeAsync()
    {
        // _executor may be null if InitializeAsync failed before materializing it
        if (_executor is not null)
            await _executor.DisposeAsync();

        await base.DisposeAsync();
    }

    /// <summary>
    /// Tests that the executor can process scheduled tasks correctly
    /// </summary>
    /// <param name="size">The number of tasks to schedule</param>
    /// <returns>A list of task execution results</returns>
    protected async Task<IReadOnlyList<int>> Works_Base(int size)
    {
        this.Trace("start");

        // run executor
        this.Trace("start executor");
        _executor.Start();

        // act
        // schedule batch of work
        this.Trace("schedule work");
        var queue = new ConcurrentQueue<int>();
        foreach (var i in Enumerable.Range(0, size))
            _executor.Schedule(() =>
            {
                queue.Enqueue(i);
                Helper.SyncLongWork();
                queue.Enqueue(i + size);
            });

        // dispose to force processing finished
        this.Trace("dispose executor");
        await _executor.DisposeAsync();

        this.Trace("done");

        return queue.ToArray();
    }

    /// <summary>
    /// Tests the executor's availability state throughout its lifecycle
    /// </summary>
    /// <returns>A task representing the test operation</returns>
    protected async Task Availability_Base()
    {
        this.Trace("start");

        // act
        // schedule batch of work
        this.Trace("schedule work");
        Parallel.For(0, 4, _ => _executor.Schedule(Helper.SyncLongWork));

        // run executor
        this.Trace("start executor");
        _executor.Start();

        // assert
        this.Trace("ensure executor is available");
        _executor.IsAvailable.IsTrue();

        // init disposal
        this.Trace("start disposal");
        var disposalTask = _executor.DisposeAsync();

        // assert
        this.Trace("ensure executor is not available");
        _executor.IsAvailable.IsFalse();

        this.Trace("ensure executor fails to schedule when not available");
        _executor.Schedule(() => { }).IsFalse();

        // cleanup
        this.Trace("await disposal");
        await disposalTask;

        this.Trace("done");
    }

    /// <summary>
    /// Tests that the executor handles task failures gracefully
    /// </summary>
    /// <returns>A task representing the test operation</returns>
    protected async Task HandlesFailure_Base()
    {
        this.Trace("start");

        // arrange
        var successes = 0;
        var failures = 0;

        // act
        // schedule batch of work
        this.Trace("schedule work");
        Parallel.For(
            0,
            10,
            i =>
                _executor.Schedule(async () =>
                {
                    await Task.Delay(10);
                    if (i % 5 == 0)
                    {
                        Interlocked.Increment(ref failures);
                        throw new Exception("Some failure");
                    }

                    Interlocked.Increment(ref successes);
                })
        );

        this.Trace("assert no events registered");
        successes.Is(0);
        failures.Is(0);

        // run executor
        this.Trace("start executor");
        _executor.Start();

        // schedule another batch of work
        this.Trace("schedule work");
        Parallel.For(
            0,
            10,
            i =>
                _executor.Schedule(async () =>
                {
                    await Task.Delay(10);
                    if (i % 5 == 0)
                    {
                        this.Trace("add failure");
                        Interlocked.Increment(ref failures);
                        throw new Exception("Some failure");
                    }

                    this.Trace("add success");
                    Interlocked.Increment(ref successes);
                })
        );

        // assert
        this.Trace("ensure executor is available");
        _executor.IsAvailable.IsTrue();

        // init disposal
        this.Trace("start disposal");
        var disposalTask = _executor.DisposeAsync();

        this.Trace("ensure executor is not available");
        _executor.IsAvailable.IsFalse();

        this.Trace("ensure executor fails to schedule when not available");
        _executor.Schedule(() => { }).IsFalse();

        this.Trace("await disposal");
        await disposalTask;

        this.Trace("assert events are registered");
        successes.Is(16);
        failures.Is(4);

        this.Trace("done");
    }

    /// <summary>
    /// Tests scheduling of synchronous action tasks
    /// </summary>
    /// <returns>A task representing the test operation</returns>
    protected async Task Schedule_SyncAction_Base()
    {
        this.Trace("start");

        // arrange
        using var cts = new CancellationTokenSource();
        var success = false;

        // act
        this.Trace("schedule work");
        _executor.Schedule(() => success = true);

        // run and dispose executor
        this.Trace("start executor");
        _executor.Start(cts.Token);

        this.Trace("dispose executor");
        await _executor.DisposeAsync();

        // assert
        this.Trace("ensure work is complete");
        success.IsTrue();

        this.Trace("done");
    }

    /// <summary>
    /// Tests scheduling of synchronous action tasks with cancellation support
    /// </summary>
    /// <returns>A task representing the test operation</returns>
    protected async Task Schedule_SyncCancellableAction_Base()
    {
        this.Trace("start");

        // arrange
        using var cts = new CancellationTokenSource();
        var isCancelled = false;

        // act
        this.Trace("schedule work");
        _executor.Schedule(ct => ct.Register(() => isCancelled = true));

        // run and dispose executor
        this.Trace("start executor");
        _executor.Start(cts.Token);

        this.Trace("dispose executor");
        await _executor.DisposeAsync();

        // assert
        this.Trace("ensure work is canceled");
        isCancelled.IsTrue();

        this.Trace("done");
    }

    /// <summary>
    /// Tests scheduling of asynchronous action tasks
    /// </summary>
    /// <returns>A task representing the test operation</returns>
    protected async Task Schedule_AsyncAction_Base()
    {
        this.Trace("start");

        // arrange
        using var cts = new CancellationTokenSource();
        var success = false;

        // act
        this.Trace("schedule work");
        _executor.Schedule(async () =>
        {
            await Task.Delay(50, CancellationToken.None);
            success = true;
        });

        // run and dispose executor
        this.Trace("start executor");
        _executor.Start(cts.Token);

        this.Trace("dispose executor");
        await _executor.DisposeAsync();

        // assert
        this.Trace("ensure work is complete");
        success.IsTrue();

        this.Trace("done");
    }

    /// <summary>
    /// Tests scheduling of asynchronous action tasks with cancellation support
    /// </summary>
    /// <returns>A task representing the test operation</returns>
    protected async Task Schedule_AsyncCancellableAction_Base()
    {
        this.Trace("start");

        // arrange
        using var cts = new CancellationTokenSource();
        var isCancelled = false;

        // act
        this.Trace("schedule work");
        _executor.Schedule(async ct =>
        {
            await Task.Delay(50, CancellationToken.None);
            ct.Register(() => isCancelled = true);
        });

        // run and dispose executor
        this.Trace("start executor");
        _executor.Start(cts.Token);

        this.Trace("dispose executor");
        await _executor.DisposeAsync();

        // assert
        this.Trace("ensure work is canceled");
        isCancelled.IsTrue();

        this.Trace("done");
    }

    /// <summary>
    /// Verifies that an exception thrown by a scheduled task is surfaced through the logger
    /// rather than silently dropped. Guards against regressions of the fire-and-forget fix
    /// that wrapped <c>RunTaskAsync</c> with a try/log block.
    /// </summary>
    /// <returns>A task representing the test operation</returns>
    protected async Task ExceptionInTask_LogsError_Base()
    {
        this.Trace("start");

        // arrange
        const string marker = "boom-from-scheduled-task";

        // act
        this.Trace("schedule throwing work");
        _executor.Schedule(() => throw new InvalidOperationException(marker));

        this.Trace("start executor");
        _executor.Start();

        this.Trace("dispose executor");
        await _executor.DisposeAsync();

        // assert — take snapshot AFTER dispose so the logs reflect the completed work
        this.Trace("ensure error logged with exception");
        var logs = Logs;
        logs.Any(x => x.Level == LogLevel.Error && x.Exception is { Message: marker }).IsTrue();

        this.Trace("done");
    }

    // -------------------------------------------------------------------------
    // Never-started disposal drain
    // -------------------------------------------------------------------------

    /// <summary>
    /// Disposing a never-started executor that has no queued tasks completes cleanly without hanging.
    /// </summary>
    /// <returns>A task representing the test operation</returns>
    protected async Task DisposeWithoutStart_NoTasks_CompletesCleanly_Base()
    {
        this.Trace("start");

        await using var executor = _getExecutor(Get<ILogger>());

        // act — dispose without ever calling Start
        this.Trace("dispose without start");
        await executor.DisposeAsync();

        // if we reach here the dispose did not hang
        this.Trace("done");
    }

    /// <summary>
    /// Disposing a never-started executor that has N queued tasks runs all N tasks.
    /// </summary>
    /// <returns>A task representing the test operation</returns>
    protected async Task DisposeWithoutStart_WithQueuedTasks_RunsAllTasks_Base()
    {
        this.Trace("start");

        const int taskCount = 5;
        await using var executor = _getExecutor(Get<ILogger>());
        var counter = 0;

        // schedule N tasks before starting
        this.Trace("schedule work");
        for (var i = 0; i < taskCount; i++)
            executor.Schedule(() => Interlocked.Increment(ref counter));

        // dispose without ever calling Start — the base should drain the queue
        this.Trace("dispose without start");
        await executor.DisposeAsync();

        // assert every task ran
        this.Trace("assert all tasks ran");
        counter.Is(taskCount);

        this.Trace("done");
    }

    /// <summary>
    /// Disposing a never-started executor whose queued task throws does not hang or propagate the exception.
    /// </summary>
    /// <returns>A task representing the test operation</returns>
    protected async Task DisposeWithoutStart_ThrowingTask_CompletesWithoutHanging_Base()
    {
        this.Trace("start");

        await using var executor = _getExecutor(Get<ILogger>());

        // schedule a task that will throw during the drain
        this.Trace("schedule throwing task");
        executor.Schedule(() => throw new InvalidOperationException("drain-throw"));

        // dispose without Start — must complete (exception is swallowed / logged, not propagated)
        this.Trace("dispose without start");
        await executor.DisposeAsync();

        // reaching here means dispose did not hang and did not propagate the exception
        this.Trace("done");
    }

    // -------------------------------------------------------------------------
    // Double-start / start-after-dispose guard
    // -------------------------------------------------------------------------

    /// <summary>
    /// Calling Start a second time throws InvalidOperationException.
    /// </summary>
    /// <returns>A task representing the test operation</returns>
    protected async Task Start_CalledTwice_ThrowsInvalidOperation_Base()
    {
        this.Trace("start");

        _executor.Start();

        Wrap.It(() => _executor.Start()).Throws<InvalidOperationException>();

        await _executor.DisposeAsync();

        this.Trace("done");
    }

    /// <summary>
    /// Calling Start after DisposeAsync throws InvalidOperationException.
    /// </summary>
    /// <returns>A task representing the test operation</returns>
    protected async Task Start_AfterDispose_ThrowsInvalidOperation_Base()
    {
        this.Trace("start");

        await _executor.DisposeAsync();

        Wrap.It(() => _executor.Start()).Throws<InvalidOperationException>();

        this.Trace("done");
    }

    // -------------------------------------------------------------------------
    // Double-dispose idempotency
    // -------------------------------------------------------------------------

    /// <summary>
    /// Calling DisposeAsync a second time after a normal Start/dispose cycle returns immediately
    /// without throwing or hanging. Exercises the State.Disposed early-return guard in ExecutorBase.
    /// </summary>
    /// <returns>A task representing the test operation</returns>
    protected async Task DoubleDispose_CompletesCleanly_Base()
    {
        this.Trace("start");

        await using var executor = _getExecutor(Get<ILogger>());

        // arrange — use a signal so the task definitely ran before we dispose
        var ran = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        this.Trace("start executor and schedule work");
        executor.Start();
        executor.Schedule(() => ran.TrySetResult());

        // wait (with a timeout guard from the test runner) until the task is confirmed complete
        this.Trace("wait for task to run");
        // VSTHRD003: ran.Task is this test's own rendezvous gate, not foreign work
#pragma warning disable VSTHRD003
        await ran.Task;
#pragma warning restore VSTHRD003

        // first dispose — normal teardown
        this.Trace("first dispose");
        await executor.DisposeAsync();

        // second dispose — must return cleanly (State.Disposed early-return guard)
        this.Trace("second dispose");
        await executor.DisposeAsync();

        // reaching here means neither call hung nor threw
        this.Trace("done");
    }

    // -------------------------------------------------------------------------
    // External-token cancellation triggers Stop
    // -------------------------------------------------------------------------

    /// <summary>
    /// Cancelling the CancellationTokenSource passed to Start causes the executor to stop:
    /// IsAvailable becomes false and subsequent Schedule calls return false.
    /// DisposeAsync completes without hanging afterwards.
    /// </summary>
    /// <returns>A task representing the test operation</returns>
    protected async Task ExternalTokenCancel_StopsExecutor_Base()
    {
        this.Trace("start");

        await using var executor = _getExecutor(Get<ILogger>());
        using var cts = new CancellationTokenSource();

        // arrange — signal that proves at least one task was picked up before we cancel
        var taskStarted = new SemaphoreSlim(0, 1);
        // ManualResetEventSlim (not a Task gate): the held task blocks on a sync primitive that
        // Set() releases directly, so it never sync-over-async-waits on a Task from the worker fiber
        using var taskRelease = new ManualResetEventSlim(false);

        this.Trace("schedule work");
        executor.Schedule(() =>
        {
            taskStarted.Release();
            taskRelease.Wait();
        });

        this.Trace("start executor with external token");
        executor.Start(cts.Token);

        // wait until the task is actually running so the executor is in a real Started state
        this.Trace("wait for task to start");
        await taskStarted.WaitAsync(TestContext.Current.CancellationToken);

        // act — cancel the external token; Stop() fires synchronously inside Cancel()
        this.Trace("cancel external token");
        await cts.CancelAsync();

        // assert — executor is no longer available
        this.Trace("assert executor is not available");
        executor.IsAvailable.IsFalse();

        this.Trace("assert scheduling returns false");
        executor.Schedule(() => { }).IsFalse();

        // unblock the running task so DisposeAsync can drain
        this.Trace("release running task");
        taskRelease.Set();

        // assert — dispose completes without hanging
        this.Trace("dispose executor");
        await executor.DisposeAsync();

        this.Trace("done");
    }

    // -------------------------------------------------------------------------
    // Graceful drain on external-token cancellation
    // -------------------------------------------------------------------------

    /// <summary>
    /// Tasks queued before an external-token cancellation must still be drained and run after Stop().
    /// Schedules N tasks: the first blocks until released (proving the executor is live), the rest
    /// are purely queued. After cancellation and release, DisposeAsync must complete with counter == N.
    /// </summary>
    /// <returns>A task representing the test operation</returns>
    protected async Task ExternalTokenCancel_DrainsQueuedTasks_Base()
    {
        this.Trace("start");

        const int taskCount = 4;
        await using var executor = _getExecutor(Get<ILogger>());
        using var cts = new CancellationTokenSource();

        var counter = 0;

        // task 0 signals when it has started (so we know the executor is truly running),
        // then blocks until taskRelease is set
        using var taskStarted = new SemaphoreSlim(0, 1);
        using var taskRelease = new ManualResetEventSlim(false);

        // task 0: signal started, block, then increment
        this.Trace("schedule blocking task 0");
        executor.Schedule(() =>
        {
            taskStarted.Release();
            taskRelease.Wait();
            Interlocked.Increment(ref counter);
        });

        // tasks 1-3: just increment
        this.Trace("schedule remaining tasks");
        for (var i = 1; i < taskCount; i++)
            executor.Schedule(() => Interlocked.Increment(ref counter));

        this.Trace("start executor with external token");
        executor.Start(cts.Token);

        // wait until task 0 is definitely running
        this.Trace("wait for task 0 to start");
        await taskStarted.WaitAsync(TestContext.Current.CancellationToken);

        // cancel — triggers Stop(), which marks the executor unavailable and completes the writer;
        // tasks 1-3 are still buffered in the channel at this point
        this.Trace("cancel external token");
        await cts.CancelAsync();

        // unblock task 0 so the drain loop can run tasks 1-3
        this.Trace("release blocking task");
        taskRelease.Set();

        // DisposeAsync awaits _runTcs.Task which completes only after every in-flight task
        // (including the drained ones) has called CompleteTask — so counter is stable here
        this.Trace("dispose executor");
        await executor.DisposeAsync();

        // assert every scheduled task ran exactly once
        this.Trace("assert all tasks ran");
        counter.Is(taskCount);

        this.Trace("done");
    }
}
