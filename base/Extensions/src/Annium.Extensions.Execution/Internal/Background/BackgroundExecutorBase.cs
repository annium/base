using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Extensions.Execution.Internal.Background;

internal abstract class BackgroundExecutorBase : IBackgroundExecutor, ILogSubject
{
    public ILogger Logger { get; }

    public bool IsAvailable
    {
        get
        {
            lock (_locker)
                return _state <= State.Started;
        }
    }

    protected readonly CancellationTokenSource Cts = new();
    private readonly object _locker = new();
    private readonly ChannelWriter<Delegate> _taskWriter;
    private readonly ChannelReader<Delegate> _taskReader;
    private readonly TaskCompletionSource _runTcs = new();
    private ConfiguredTaskAwaitable _runTask = Task.CompletedTask.ConfigureAwait(false);
    private State _state = State.Created;
    private int _taskCounter;

    protected BackgroundExecutorBase(ILogger logger)
    {
        Logger = logger;
        var taskChannel = Channel.CreateUnbounded<Delegate>(
            new UnboundedChannelOptions
            {
                AllowSynchronousContinuations = true,
                SingleWriter = false,
                SingleReader = true
            }
        );
        _taskWriter = taskChannel.Writer;
        _taskReader = taskChannel.Reader;
    }

    public void Schedule(Action task)
    {
        ScheduleTask(task);
    }

    public void Schedule(Action<CancellationToken> task)
    {
        ScheduleTask(task);
    }

    public void Schedule(Func<ValueTask> task)
    {
        ScheduleTask(task);
    }

    public void Schedule(Func<CancellationToken, ValueTask> task)
    {
        ScheduleTask(task);
    }

    public bool TrySchedule(Action task)
    {
        return TryScheduleTask(task);
    }

    public bool TrySchedule(Action<CancellationToken> task)
    {
        return TryScheduleTask(task);
    }

    public bool TrySchedule(Func<ValueTask> task)
    {
        return TryScheduleTask(task);
    }

    public bool TrySchedule(Func<CancellationToken, ValueTask> task)
    {
        return TryScheduleTask(task);
    }

    public void Start(CancellationToken ct = default)
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
        _runTask = Task.Run(Run, CancellationToken.None).ConfigureAwait(false);

        this.Trace("done");
    }

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
        Cts.Cancel();

        this.Trace("complete task writer");
        _taskWriter.TryComplete();

        this.Trace("wait for task(s) to run");
        await _runTask;

        this.Trace("wait for reader completion");
        await _taskReader.Completion.ConfigureAwait(false);

        this.Trace("try finish to ensure complete if all tasks already completed");
        TryFinish(_taskCounter);

        this.Trace("wait for task(s) to finish");
        await _runTcs.Task;

        this.Trace("done");
    }

    protected abstract Task RunTask(Delegate task);

    protected void CompleteTask(Delegate task)
    {
        var taskCounter = Interlocked.Decrement(ref _taskCounter);
        this.Trace("complete task {id} ({num})", task.GetFullId(), taskCounter);
        TryFinish(taskCounter);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ScheduleTask(Delegate task)
    {
        if (!TryScheduleTask(task))
            throw new InvalidOperationException("Task schedule failed");
    }

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

    private async Task Run()
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
                await RunTask(task);
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
            await RunTask(task);
        }

        this.Trace("done");
    }

    private void Stop()
    {
        this.Trace("start");

        lock (_locker)
        {
            if (_state is State.Stopped or State.Disposed)
            {
                this.Trace($"Executor is already {_state}");
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
        Disposed = 3
    }
}
