using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Execution.Background.Internal;

/// <summary>
/// Abstract base class for background task executors
/// </summary>
internal abstract class ExecutorBase : IExecutor, ILogSubject
{
    /// <summary>
    /// Gets the logger instance for this executor
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Gets a value indicating whether the executor is available to schedule new tasks
    /// </summary>
    public bool IsAvailable
    {
        get
        {
            lock (_locker)
                return _state <= State.Started;
        }
    }

    /// <summary>
    /// Cancellation token source for managing executor lifecycle
    /// </summary>
    protected readonly CancellationTokenSource Cts = new();

    /// <summary>
    /// Lock for synchronizing state changes
    /// </summary>
    private readonly Lock _locker = new();

    /// <summary>
    /// Channel writer for adding tasks to the execution queue
    /// </summary>
    private readonly ChannelWriter<Delegate> _taskWriter;

    /// <summary>
    /// Channel reader for consuming tasks from the execution queue
    /// </summary>
    private readonly ChannelReader<Delegate> _taskReader;

    /// <summary>
    /// Task completion source for signaling when all tasks are complete
    /// </summary>
    // RunContinuationsAsynchronously: the last task to finish completes this gate from CompleteTask on a
    // thread-pool fiber; without it, DisposeAsync's continuation (DisposeResourcesAsync / Cts.Dispose) would
    // run inline on that fiber instead of the disposer's context
    private readonly TaskCompletionSource _runTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>
    /// The main execution task
    /// </summary>
    private ConfiguredTaskAwaitable _runTask = Task.CompletedTask.ConfigureAwait(false);

    /// <summary>
    /// Current state of the executor
    /// </summary>
    private State _state = State.Created;

    /// <summary>
    /// Counter tracking the number of running tasks
    /// </summary>
    private int _taskCounter;

    /// <summary>
    /// Registration of <see cref="Stop"/> on the caller's start token; disposed in
    /// <see cref="DisposeAsync"/> so a non-default token source does not retain this executor.
    /// </summary>
    private CancellationTokenRegistration _stopRegistration;

    /// <summary>
    /// Initializes a new instance of the ExecutorBase class
    /// </summary>
    /// <param name="logger">The logger instance</param>
    protected ExecutorBase(ILogger logger)
    {
        Logger = logger;
        var taskChannel = Channel.CreateUnbounded<Delegate>(
            new UnboundedChannelOptions
            {
                AllowSynchronousContinuations = true,
                SingleWriter = false,
                SingleReader = true,
            }
        );
        _taskWriter = taskChannel.Writer;
        _taskReader = taskChannel.Reader;
    }

    /// <summary>
    /// Schedules a synchronous task for execution
    /// </summary>
    /// <param name="task">The task to schedule</param>
    /// <returns>True if the task was successfully scheduled, false otherwise</returns>
    public bool Schedule(Action task) => TryScheduleTask(task);

    /// <summary>
    /// Schedules a synchronous task for execution with cancellation support
    /// </summary>
    /// <param name="task">The task to schedule</param>
    /// <returns>True if the task was successfully scheduled, false otherwise</returns>
    public bool Schedule(Action<CancellationToken> task) => TryScheduleTask(task);

    /// <summary>
    /// Schedules an asynchronous task for execution
    /// </summary>
    /// <param name="task">The task to schedule</param>
    /// <returns>True if the task was successfully scheduled, false otherwise</returns>
    public bool Schedule(Func<ValueTask> task) => TryScheduleTask(task);

    /// <summary>
    /// Schedules an asynchronous task for execution with cancellation support
    /// </summary>
    /// <param name="task">The task to schedule</param>
    /// <returns>True if the task was successfully scheduled, false otherwise</returns>
    public bool Schedule(Func<CancellationToken, ValueTask> task) => TryScheduleTask(task);

    /// <summary>
    /// Starts the executor
    /// </summary>
    /// <param name="ct">The cancellation token</param>
    /// <returns>The executor instance</returns>
    public IExecutor Start(CancellationToken ct = default)
    {
        this.Trace("start");

        lock (_locker)
        {
            // ensure is in created state
            if (_state is not State.Created)
                throw new InvalidOperationException($"Executor is already {_state}");

            this.Trace("set state to started");
            _state = State.Started;
        }

        // change to state to unavailable
        this.Trace("register stop on token cancellation");
        _stopRegistration = ct.Register(Stop);

        this.Trace("run");
        _runTask = Task.Run(RunAsync, CancellationToken.None).ConfigureAwait(false);

        this.Trace("done");

        return this;
    }

    /// <summary>
    /// Disposes the executor and waits for all tasks to complete
    /// </summary>
    /// <returns>A task representing the disposal operation</returns>
    public async ValueTask DisposeAsync()
    {
        this.Trace("start");

        bool wasStarted;
        lock (_locker)
        {
            if (_state is State.Disposed)
            {
                this.Trace("Executor is already {state}", _state);
                return;
            }

            // RunAsync (the channel's single reader) runs only once Start has moved the executor past
            // Created. If we are disposing a never-started executor, DisposeAsync must drain the channel
            // itself — otherwise buffered tasks are never consumed and _taskReader.Completion never fires.
            wasStarted = _state is not State.Created;

            this.Trace("set state to disposed");
            _state = State.Disposed;
        }

        this.Trace("cancel cts");
        await Cts.CancelAsync();

        this.Trace("complete task writer");
        _taskWriter.TryComplete();

        this.Trace("wait for task(s) to run");
        await _runTask;

        if (!wasStarted)
        {
            this.Trace("executor was never started - drain pending tasks so the channel can complete");
            await DrainPendingTasksAsync();
        }

        this.Trace("wait for reader completion");
        // VSTHRD003: _taskReader.Completion is the channel's own lifecycle sentinel (completed by
        // TryComplete above), not foreign work — awaiting it directly is the correct drain pattern
#pragma warning disable VSTHRD003
        await _taskReader.Completion.ConfigureAwait(false);
#pragma warning restore VSTHRD003

        this.Trace("try finish to ensure complete if all tasks already completed");
        TryFinish(_taskCounter);

        this.Trace("wait for task(s) to finish");
        // VSTHRD003: _runTcs is this executor's own disposal drain-gate (set by CompleteTask/TryFinish
        // when the last in-flight task finishes), not foreign work — DisposeAsync must await it directly
#pragma warning disable VSTHRD003
        await _runTcs.Task;
#pragma warning restore VSTHRD003

        this.Trace("dispose subclass-owned resources");
        await DisposeResourcesAsync();

        this.Trace("unregister stop from start token");
        await _stopRegistration.DisposeAsync();

        this.Trace("dispose cts");
        Cts.Dispose();

        this.Trace("done");
    }

    /// <summary>
    /// Runs a task asynchronously. Implementation varies by executor type
    /// </summary>
    /// <param name="task">The task to run</param>
    /// <returns>A task representing the execution</returns>
    protected abstract Task RunTaskAsync(Delegate task);

    /// <summary>
    /// Runs <paramref name="task"/> on a background thread-pool fiber via the supplied
    /// <paramref name="start"/> delegate, marking it complete afterwards and surfacing any fault
    /// through the logger. Shared by the fire-and-forget executors (parallel / concurrent); returns a
    /// completed task immediately so the dispatch loop is not blocked.
    /// </summary>
    /// <param name="start">The differentiated per-executor start logic (e.g. semaphore-gated or not)</param>
    /// <param name="task">The task to run</param>
    /// <returns>A completed task</returns>
    protected Task RunTaskInBackgroundAsync(Func<Delegate, Task> start, Delegate task)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await start(task);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                this.Error(ex);
            }
            finally
            {
                // always decrement the drain counter so DisposeAsync's _runTcs gate completes even if
                // start propagates (it currently swallows internally, but the contract must not rely on that)
                CompleteTask(task);
            }
        });

        return Task.CompletedTask;
    }

    /// <summary>
    /// Disposes resources owned by a subclass. Invoked once at the end of <see cref="DisposeAsync"/>,
    /// after all scheduled tasks have drained and completed, so subclass-owned resources (e.g. a
    /// semaphore) are released only when no scheduled work can still reference them. Base-owned
    /// resources (the <see cref="Cts"/>) are disposed by <see cref="DisposeAsync"/> itself.
    /// </summary>
    /// <returns>A task representing the resource disposal</returns>
    protected virtual ValueTask DisposeResourcesAsync() => ValueTask.CompletedTask;

    /// <summary>
    /// Marks a task as completed and decrements the task counter
    /// </summary>
    /// <param name="task">The completed task</param>
    protected void CompleteTask(Delegate task)
    {
        var taskCounter = Interlocked.Decrement(ref _taskCounter);
        this.Trace("complete task {id} ({num})", task.GetFullId(), taskCounter);
        TryFinish(taskCounter);
    }

    /// <summary>
    /// Attempts to schedule a task for execution
    /// </summary>
    /// <param name="task">The task to schedule</param>
    /// <returns>True if the task was successfully scheduled, false if the executor is not available</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool TryScheduleTask(Delegate task)
    {
        lock (_locker)
        {
            if (_state is not (State.Created or State.Started))
            {
                this.Trace("Executor is already {state}", _state);
                return false;
            }
        }

        this.Trace<string>("schedule task {id}", task.GetFullId());
        if (_taskWriter.TryWrite(task))
            return true;

        this.Trace<string>("schedule task {id} failed - writer is already complete", task.GetFullId());

        return false;
    }

    /// <summary>
    /// Main execution loop that processes scheduled tasks
    /// </summary>
    /// <returns>A task representing the execution loop</returns>
    private async Task RunAsync()
    {
        this.Trace("start");

        // normal mode - runs task immediately or waits for one
        this.Trace("run normal mode while executor is available");
        while (IsAvailable)
        {
            try
            {
                this.Trace("await for task");
                var task = await _taskReader.ReadAsync(Cts.Token);

                this.Trace("run task {id} ({num})", task.GetFullId(), Interlocked.Increment(ref _taskCounter));
                await RunTaskAsync(task);
            }
            catch (ChannelClosedException)
            {
                this.Trace("channel closed");
                break;
            }
            catch (OperationCanceledException)
            {
                this.Trace("operation canceled");
                break;
            }
        }

        // shutdown mode - runs only left tasks
        this.Trace("run tasks left");
        await DrainPendingTasksAsync();

        this.Trace("done");
    }

    /// <summary>
    /// Drains and runs every task still buffered in the channel. Used both by the shutdown phase of
    /// <see cref="RunAsync"/> and by <see cref="DisposeAsync"/> when the executor is disposed without
    /// ever being started. The channel is configured single-reader, so the two callers are mutually
    /// exclusive: RunAsync drains when the executor was started, DisposeAsync drains when it was not.
    /// </summary>
    /// <returns>A task representing the drain operation</returns>
    private async Task DrainPendingTasksAsync()
    {
        while (true)
        {
            if (!_taskReader.TryRead(out var task))
                break;

            this.Trace("run task {id} ({num})", task.GetFullId(), Interlocked.Increment(ref _taskCounter));
            await RunTaskAsync(task);
        }
    }

    /// <summary>
    /// Stops the executor and prevents new tasks from being scheduled
    /// </summary>
    private void Stop()
    {
        this.Trace("start");

        lock (_locker)
        {
            if (_state is State.Stopped or State.Disposed)
            {
                this.Trace("Executor is already {state}", _state);
                return;
            }

            _state = State.Stopped;
        }

        this.Trace("cancel cts");
        Cts.Cancel();

        this.Trace("complete task writer");
        _taskWriter.TryComplete();

        this.Trace("done");
    }

    /// <summary>
    /// Attempts to finish the executor if no tasks are running and the executor is not available
    /// </summary>
    /// <param name="taskCounter">The current number of running tasks</param>
    private void TryFinish(int taskCounter)
    {
        if (IsAvailable || taskCounter != 0)
        {
            this.Trace("not finishing: isAvailable: {IsAvailable}, tasks: {taskCounter}", IsAvailable, taskCounter);
            return;
        }

        this.Trace("try complete run tcs");
        _runTcs.TrySetResult();
    }

    /// <summary>
    /// Lifecycle states an executor moves through, in order.
    /// </summary>
    private enum State : byte
    {
        /// <summary>
        /// Constructed but not started — scheduling is rejected.
        /// </summary>
        Created = 0,

        /// <summary>
        /// Running — scheduled tasks are accepted and executed.
        /// </summary>
        Started = 1,

        /// <summary>
        /// Stopped accepting new tasks; already-scheduled ones are still draining.
        /// </summary>
        Stopped = 2,

        /// <summary>
        /// Fully disposed — the drain has completed and all resources are released.
        /// </summary>
        Disposed = 3,
    }
}
