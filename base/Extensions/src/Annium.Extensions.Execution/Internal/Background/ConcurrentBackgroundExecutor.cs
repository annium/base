using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Extensions.Execution.Internal.Background;

// ReSharper disable once UnusedTypeParameter
internal class ConcurrentBackgroundExecutor<TSource> : BackgroundExecutorBase
{
    private int _taskCounter;
    private readonly ConcurrentBag<Delegate> _backlog = new();
    private readonly SemaphoreSlim _gate;
    private readonly TaskCompletionSource<object> _tcs = new();

    public ConcurrentBackgroundExecutor(
        int parallelism,
        ILogger logger
    ) : base(logger)
    {
        _gate = new SemaphoreSlim(parallelism, parallelism);
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
            StartTask(task).ContinueWith(CompleteTask);
        else
            _backlog.Add(task);
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

    private async Task StartTask(Delegate task)
    {
        try
        {
            Interlocked.Increment(ref _taskCounter);
            await _gate.WaitAsync();
            await Helper.RunTaskInBackground(task, Cts.Token);
        }
        finally
        {
            _gate.Release();
        }
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