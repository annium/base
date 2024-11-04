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

    protected override Task RunTaskAsync(Delegate task)
    {
        StartTaskAsync(task).ContinueWith(_ => CompleteTask(task)).GetAwaiter();

        return Task.CompletedTask;
    }

    private async Task StartTaskAsync(Delegate task)
    {
        try
        {
            await _gate.WaitAsync();
            await Helper.RunTaskInBackgroundAsync(task, Cts.Token);
        }
        catch (OperationCanceledException)
        {
            this.Trace("task canceled");
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
