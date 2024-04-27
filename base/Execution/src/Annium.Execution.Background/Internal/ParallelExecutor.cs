using System;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Execution.Background.Internal;

// ReSharper disable once UnusedTypeParameter
internal class ParallelExecutor<TSource> : ExecutorBase
{
    public ParallelExecutor(ILogger logger)
        : base(logger) { }

    protected override Task RunTask(Delegate task)
    {
        StartTask(task).ContinueWith(_ => CompleteTask(task));

        return Task.CompletedTask;
    }

    private async Task StartTask(Delegate task)
    {
        try
        {
            await Helper.RunTaskInBackground(task, Cts.Token);
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
