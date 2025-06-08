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
    private readonly TaskCompletionSource _runTcs = new();

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
    public bool Schedule(Action task)
    {
        return TryScheduleTask(task);
    }

    /// <summary>
    /// Schedules a synchronous task for execution with cancellation support
    /// </summary>
    /// <param name="task">The task to schedule</param>
    /// <returns>True if the task was successfully scheduled, false otherwise</returns>
    public bool Schedule(Action<CancellationToken> task)
    {
        return TryScheduleTask(task);
    }

    /// <summary>
    /// Schedules an asynchronous task for execution
    /// </summary>
    /// <param name="task">The task to schedule</param>
    /// <returns>True if the task was successfully scheduled, false otherwise</returns>
    public bool Schedule(Func<ValueTask> task)
    {
        return TryScheduleTask(task);
    }

    /// <summary>
    /// Schedules an asynchronous task for execution with cancellation support
    /// </summary>
    /// <param name="task">The task to schedule</param>
    /// <returns>True if the task was successfully scheduled, false otherwise</returns>
    public bool Schedule(Func<CancellationToken, ValueTask> task)
    {
        return TryScheduleTask(task);
    }

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
        ct.Register(Stop);

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

        lock (_locker)
        {
            if (_state is State.Disposed)
            {
                this.Trace("Executor is already {state}", _state);
                return;
            }

            this.Trace("set state to disposed");
            _state = State.Disposed;
        }

        this.Trace("cancel cts");
        await Cts.CancelAsync();

        this.Trace("complete task writer");
        _taskWriter.TryComplete();

        this.Trace("wait for task(s) to run");
        await _runTask;

        this.Trace("wait for reader completion");
#pragma warning disable VSTHRD003
        await _taskReader.Completion.ConfigureAwait(false);
#pragma warning restore VSTHRD003

        this.Trace("try finish to ensure complete if all tasks already completed");
        TryFinish(_taskCounter);

        this.Trace("wait for task(s) to finish");
#pragma warning disable VSTHRD003
        await _runTcs.Task;
#pragma warning restore VSTHRD003

        this.Trace("done");
    }

    /// <summary>
    /// Runs a task asynchronously. Implementation varies by executor type
    /// </summary>
    /// <param name="task">The task to run</param>
    /// <returns>A task representing the execution</returns>
    protected abstract Task RunTaskAsync(Delegate task);

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
        while (true)
        {
            if (!_taskReader.TryRead(out var task))
                break;

            this.Trace("run task {id} ({num})", task.GetFullId(), Interlocked.Increment(ref _taskCounter));
            await RunTaskAsync(task);
        }

        this.Trace("done");
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

    private enum State : byte
    {
        Created = 0,
        Started = 1,
        Stopped = 2,
        Disposed = 3,
    }
}
