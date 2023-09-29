using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Extensions.Execution.Internal.Background;

internal abstract class BackgroundExecutorBase : IBackgroundExecutor, ILogSubject
{
    public ILogger Logger { get; }
    public bool IsAvailable => _state is State.Created or State.Started;
    protected bool IsStarted => _state is State.Started;
    protected readonly CancellationTokenSource Cts = new();
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
        EnsureAvailable();

        var sl = new SpinLock();
        var lockTaken = false;
        try
        {
            sl.Enter(ref lockTaken);
            if (_state is not State.Created)
                throw new InvalidOperationException("Executor has already started");
            _state = State.Started;
        }
        finally
        {
            if (lockTaken)
                sl.Exit();
        }

        // change to state to unavailable
        ct.Register(Stop);

        this.Trace("run");
        HandleStart();
    }

    public async ValueTask DisposeAsync()
    {
        var sl = new SpinLock();
        var lockTaken = false;
        try
        {
            sl.Enter(ref lockTaken);
            if (_state is State.Disposed)
            {
                this.Trace("already disposed");
                return;
            }

            _state = State.Disposed;
        }
        finally
        {
            if (lockTaken)
                sl.Exit();
        }

        this.Trace("start");
        Stop();
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
        EnsureAvailable();
        ScheduleTaskCore(task);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool TryScheduleTask(Delegate task)
    {
        if (!IsAvailable)
            return false;

        ScheduleTaskCore(task);

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureAvailable()
    {
        if (!IsAvailable)
            throw new InvalidOperationException("Executor is not available already");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Stop()
    {
        var sl = new SpinLock();
        var lockTaken = false;
        try
        {
            sl.Enter(ref lockTaken);
            if (_state is State.Stopped)
            {
                this.Trace("already stopped");
                return;
            }

            _state = State.Stopped;
        }
        finally
        {
            if (lockTaken)
                sl.Exit();
        }

        this.Trace("start");
        Cts.Cancel();
        HandleStop();
        this.Trace("done");
    }

    private enum State : byte
    {
        Created,
        Started,
        Stopped,
        Disposed
    }
}