using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Execution.Background.Internal;

// ReSharper disable once UnusedTypeParameter
/// <summary>
/// Executor that runs tasks with controlled concurrency using a semaphore
/// </summary>
/// <typeparam name="TSource">Source type for executor identification</typeparam>
internal class ConcurrentExecutor<TSource> : ExecutorBase
{
    /// <summary>
    /// Semaphore controlling the maximum number of concurrent tasks
    /// </summary>
    private readonly SemaphoreSlim _gate;

    /// <summary>
    /// Initializes a new instance of the ConcurrentExecutor class
    /// </summary>
    /// <param name="parallelism">Maximum number of concurrent tasks</param>
    /// <param name="logger">The logger instance</param>
    public ConcurrentExecutor(int parallelism, ILogger logger)
        : base(logger)
    {
        _gate = new SemaphoreSlim(parallelism, parallelism);
    }

    /// <summary>
    /// Runs a task asynchronously with concurrency control
    /// </summary>
    /// <param name="task">The task to run</param>
    /// <returns>A completed task</returns>
    protected override Task RunTaskAsync(Delegate task)
    {
        StartTaskAsync(task).ContinueWith(_ => CompleteTask(task)).GetAwaiter();

        return Task.CompletedTask;
    }

    /// <summary>
    /// Starts a task with semaphore-controlled concurrency
    /// </summary>
    /// <param name="task">The task to start</param>
    /// <returns>A task representing the execution</returns>
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
