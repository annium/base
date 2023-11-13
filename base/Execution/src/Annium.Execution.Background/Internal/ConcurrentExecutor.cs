using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Execution.Background.Internal;

// ReSharper disable once UnusedTypeParameter
internal class ConcurrentExecutor<TSource> : ExecutorBase
{
    private readonly SemaphoreSlim _gate;

    public ConcurrentExecutor(int parallelism, ILogger logger)
        : base(logger)
    {
        _gate = new SemaphoreSlim(parallelism, parallelism);
    }

    protected override Task RunTask(Delegate task)
    {
        StartTask(task).ContinueWith(_ => CompleteTask(task));

        return Task.CompletedTask;
    }

    private async Task StartTask(Delegate task)
    {
        try
        {
            await _gate.WaitAsync();
            await Helper.RunTaskInBackground(task, Cts.Token);
        }
        catch (Exception e)
        {
            this.Error(e);
        }
        finally
        {
            _gate.Release();
        }
    }
}