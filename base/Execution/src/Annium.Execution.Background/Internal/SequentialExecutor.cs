using System;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Execution.Background.Internal;

// ReSharper disable once UnusedTypeParameter
/// <summary>
/// Executor that runs tasks sequentially one after another
/// </summary>
/// <typeparam name="TSource">Source type for executor identification</typeparam>
internal class SequentialExecutor<TSource> : ExecutorBase
{
    /// <summary>
    /// Initializes a new instance of the SequentialExecutor class
    /// </summary>
    /// <param name="logger">The logger instance</param>
    public SequentialExecutor(ILogger logger)
        : base(logger) { }

    /// <summary>
    /// Runs a task asynchronously in the foreground sequentially
    /// </summary>
    /// <param name="task">The task to run</param>
    /// <returns>A task representing the execution</returns>
    protected override async Task RunTaskAsync(Delegate task)
    {
        try
        {
            await Helper.RunTaskInForegroundAsync(task, Cts.Token);
        }
        catch (OperationCanceledException)
        {
            this.Trace("task canceled");
        }
        catch (Exception e)
        {
            this.Error(e);
        }

        CompleteTask(task);
    }
}
