using System;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Execution.Background.Internal;

// ReSharper disable once UnusedTypeParameter
/// <summary>
/// Executor that runs tasks in parallel without concurrency limits
/// </summary>
/// <typeparam name="TSource">Source type for executor identification</typeparam>
internal class ParallelExecutor<TSource> : ExecutorBase
{
    /// <summary>
    /// Initializes a new instance of the ParallelExecutor class
    /// </summary>
    /// <param name="logger">The logger instance</param>
    public ParallelExecutor(ILogger logger)
        : base(logger) { }

    /// <summary>
    /// Runs a task asynchronously in parallel
    /// </summary>
    /// <param name="task">The task to run</param>
    /// <returns>A completed task</returns>
    protected override Task RunTaskAsync(Delegate task)
    {
        StartTaskAsync(task).ContinueWith(_ => CompleteTask(task)).GetAwaiter();

        return Task.CompletedTask;
    }

    /// <summary>
    /// Starts a task in the background
    /// </summary>
    /// <param name="task">The task to start</param>
    /// <returns>A task representing the execution</returns>
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
