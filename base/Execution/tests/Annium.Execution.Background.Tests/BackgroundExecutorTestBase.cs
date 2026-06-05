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
        var cts = new CancellationTokenSource();
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
        var cts = new CancellationTokenSource();
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
        var cts = new CancellationTokenSource();
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
        var cts = new CancellationTokenSource();
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
    /// <param name="getLogs">Accessor that snapshots the test's <see cref="TestBase.Logs"/> at call time</param>
    /// <returns>A task representing the test operation</returns>
    protected async Task ExceptionInTask_LogsError_Base(Func<IReadOnlyList<LogMessage<DefaultLogContext>>> getLogs)
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
        var logs = getLogs();
        logs.Any(x => x.Level == LogLevel.Error && x.Exception is { Message: marker }).IsTrue();

        this.Trace("done");
    }
}
