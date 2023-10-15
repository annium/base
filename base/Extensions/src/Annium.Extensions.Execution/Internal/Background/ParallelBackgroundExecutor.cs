using System;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Extensions.Execution.Internal.Background;

// ReSharper disable once UnusedTypeParameter
internal class ParallelBackgroundExecutor<TSource> : BackgroundExecutorBase
{
    public ParallelBackgroundExecutor(
        ILogger logger
    ) : base(logger)
    {
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
            await Helper.RunTaskInBackground(task, Cts.Token);
        }
        catch (Exception e)
        {
            this.Error(e);
        }
    }
}