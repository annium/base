using System;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Execution.Background.Internal;

// ReSharper disable once UnusedTypeParameter
internal class SequentialExecutor<TSource> : ExecutorBase
{
    public SequentialExecutor(ILogger logger)
        : base(logger) { }

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
