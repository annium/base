using System;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Extensions.Execution.Internal.Background;

// ReSharper disable once UnusedTypeParameter
internal class SequentialBackgroundExecutor<TSource> : BackgroundExecutorBase
{
    public SequentialBackgroundExecutor(
        ILogger logger
    ) : base(logger)
    {
    }

    protected override async Task RunTask(Delegate task)
    {
        try
        {
            await Helper.RunTaskInForeground(task, Cts.Token);
        }
        catch (Exception e)
        {
            this.Error(e);
        }

        CompleteTask(task);
    }
}