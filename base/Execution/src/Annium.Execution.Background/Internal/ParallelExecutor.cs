using System;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Execution.Background.Internal;

// ReSharper disable once UnusedTypeParameter
internal class ParallelExecutor<TSource> : ExecutorBase
{
    public ParallelExecutor(ILogger logger)
        : base(logger) { }

    protected override Task RunTaskAsync(Delegate task)
    {
        StartTaskAsync(task).ContinueWith(_ => CompleteTask(task)).GetAwaiter();

        return Task.CompletedTask;
    }

    private async Task StartTaskAsync(Delegate task)
    {
        try
        {
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
    }
}
