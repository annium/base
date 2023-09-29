using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Extensions.Execution.Internal.Background;

// ReSharper disable once UnusedTypeParameter
internal class ParallelBackgroundExecutor<TSource> : BackgroundExecutorBase
{
    private int _taskCounter;
    private readonly ConcurrentBag<Delegate> _backlog = new();
    private readonly TaskCompletionSource<object> _tcs = new();

    public ParallelBackgroundExecutor(
        ILogger logger
    ) : base(logger)
    {
    }

    protected override void HandleStart()
    {
        while (_backlog.TryTake(out var task))
            ScheduleTaskCore(task);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void ScheduleTaskCore(Delegate task)
    {
        if (IsStarted)
        {
            Interlocked.Increment(ref _taskCounter);
            Helper.RunTaskInBackground(task, Cts.Token).ContinueWith(CompleteTask);
        }
        else
        {
            _backlog.Add(task);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void HandleStop()
    {
        this.Trace("isAvailable: {isAvailable}, tasks: {taskCounter}", IsAvailable, _taskCounter);
        TryFinish();
    }

    protected override async ValueTask HandleDisposeAsync()
    {
        this.Trace("wait for {taskCounter} task(s) to finish", _taskCounter);
        await _tcs.Task;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CompleteTask(Task task)
    {
        if (task.Exception is not null)
            this.Error(task.Exception);

        Interlocked.Decrement(ref _taskCounter);
        TryFinish();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void TryFinish()
    {
        if (!IsAvailable && Volatile.Read(ref _taskCounter) == 0)
            _tcs.TrySetResult(new object());
    }
}