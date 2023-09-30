using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Extensions.Execution.Internal.Background;

internal abstract class BackgroundExecutorBase : IBackgroundExecutor, ILogSubject
{
    public ILogger Logger { get; }

    public bool IsAvailable => _state <= State.Started;

    protected bool IsStarted => _state is State.Started;

    protected readonly CancellationTokenSource Cts = new();
    private readonly object _locker = new();
    private State _state = State.Created;

    protected BackgroundExecutorBase(ILogger logger)
    {
        Logger = logger;
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
        lock (_locker)
        {
            // ensure is in created state
            if (_state is not State.Created)
                throw new InvalidOperationException($"Executor is already {_state}");
            _state = State.Started;
        }

        // change to state to unavailable
        ct.Register(Stop);

        this.Trace("run");
        HandleStart();
    }

    public async ValueTask DisposeAsync()
    {
        lock (_locker)
        {
            if (_state is State.Disposed)
            {
                this.Trace("already disposed");
                return;
            }

            _state = State.Disposed;
        }

        this.Trace("start");

        this.Trace("cancel cts");
        Cts.Cancel();

        this.Trace("handle stop");
        HandleStop();

        this.Trace("handle dispose");
        await HandleDisposeAsync();

        this.Trace("done");
    }

    protected abstract void HandleStart();
    protected abstract void ScheduleTaskCore(Delegate task);
    protected abstract void HandleStop();
    protected abstract ValueTask HandleDisposeAsync();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ScheduleTask(Delegate task)
    {
        lock (_locker)
        {
            if (_state is not (State.Created or State.Started))
                throw new InvalidOperationException($"Executor is already {_state}");
        }

        this.Trace("schedule task");
        ScheduleTaskCore(task);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool TryScheduleTask(Delegate task)
    {
        lock (_locker)
        {
            if (_state is not (State.Created or State.Started))
                return false;
        }

        this.Trace("schedule task");
        ScheduleTaskCore(task);

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Stop()
    {
        lock (_locker)
        {
            if (_state is State.Stopped or State.Disposed)
            {
                this.Trace($"Executor is already {_state}");
                return;
            }

            _state = State.Stopped;
        }

        this.Trace("start");

        this.Trace("cancel cts");
        Cts.Cancel();

        this.Trace("handle stop");
        HandleStop();

        this.Trace("done");
    }

    private enum State : byte
    {
        Created = 0,
        Started = 1,
        Stopped = 2,
        Disposed = 3
    }
}