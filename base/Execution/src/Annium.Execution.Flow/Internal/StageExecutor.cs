using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Data.Operations;

namespace Annium.Execution.Flow.Internal;

/// <summary>
/// Internal implementation of stage executor for running operations with commit/rollback support
/// </summary>
internal class StageExecutor : IStageExecutor
{
    /// <summary>
    /// List of stages to execute
    /// </summary>
    private readonly List<StageInfo> _stages = new();

    /// <summary>
    /// Adds a stage with a synchronous commit operation
    /// </summary>
    /// <param name="commit">The commit operation</param>
    /// <returns>The stage executor for method chaining</returns>
    public IStageExecutor Stage(Action commit) => StageInternal(commit);

    /// <summary>
    /// Adds a stage with synchronous commit and rollback operations
    /// </summary>
    /// <param name="commit">The commit operation</param>
    /// <param name="rollback">The rollback operation</param>
    /// <returns>The stage executor for method chaining</returns>
    public IStageExecutor Stage(Action commit, Action rollback) => StageInternal(commit, rollback);

    /// <summary>
    /// Adds a stage with synchronous commit and asynchronous rollback operations
    /// </summary>
    /// <param name="commit">The commit operation</param>
    /// <param name="rollback">The rollback operation</param>
    /// <returns>The stage executor for method chaining</returns>
    public IStageExecutor Stage(Action commit, Func<Task> rollback) => StageInternal(commit, rollback);

    /// <summary>
    /// Adds a stage with an asynchronous commit operation
    /// </summary>
    /// <param name="commit">The commit operation</param>
    /// <returns>The stage executor for method chaining</returns>
    public IStageExecutor Stage(Func<Task> commit) => StageInternal(commit);

    /// <summary>
    /// Adds a stage with asynchronous commit and synchronous rollback operations
    /// </summary>
    /// <param name="commit">The commit operation</param>
    /// <param name="rollback">The rollback operation</param>
    /// <returns>The stage executor for method chaining</returns>
    public IStageExecutor Stage(Func<Task> commit, Action rollback) => StageInternal(commit, rollback);

    /// <summary>
    /// Adds a stage with asynchronous commit and rollback operations
    /// </summary>
    /// <param name="commit">The commit operation</param>
    /// <param name="rollback">The rollback operation</param>
    /// <returns>The stage executor for method chaining</returns>
    public IStageExecutor Stage(Func<Task> commit, Func<Task> rollback) => StageInternal(commit, rollback);

    /// <summary>
    /// Executes all stages and returns the result
    /// </summary>
    /// <returns>The result of the staged execution</returns>
    public async Task<IResult> RunAsync()
    {
        var result = Result.New();
        var executedStages = await CommitAsync(_stages, result);

        // if no exceptions - done
        if (result.IsOk)
            return result;

        // exception caught, rollback
        await RollbackAsync(_stages.Take(executedStages), result);

        return result;
    }

    /// <summary>
    /// Internal method for adding stages
    /// </summary>
    /// <param name="commit">The commit operation</param>
    /// <param name="rollback">The rollback operation</param>
    /// <returns>The stage executor instance</returns>
    private StageExecutor StageInternal(Delegate commit, Delegate? rollback = null)
    {
        _stages.Add(new StageInfo(commit, rollback));

        return this;
    }

    /// <summary>
    /// Commits all stages and returns the number of executed stages
    /// </summary>
    /// <param name="stages">The stages to commit</param>
    /// <param name="result">The result to store errors in</param>
    /// <returns>The number of executed stages</returns>
    private static async Task<int> CommitAsync(IEnumerable<StageInfo> stages, IResult result)
    {
        var i = 0;

        foreach (var stage in stages)
        {
            try
            {
                // count before stage run to include failed stage
                i++;

                await ExecuteAsync(stage.Commit);
            }
            catch (Exception exception)
            {
                result.Error(exception.Message);
            }
        }

        return i;
    }

    /// <summary>
    /// Rolls back the specified stages
    /// </summary>
    /// <param name="stages">The stages to rollback</param>
    /// <param name="result">The result to store errors in</param>
    /// <returns>A task representing the rollback operation</returns>
    private static async Task RollbackAsync(IEnumerable<StageInfo> stages, IResult result)
    {
        foreach (var stage in stages)
        {
            try
            {
                await ExecuteAsync(stage.Rollback);
            }
            catch (Exception exception)
            {
                result.Error(exception.Message);
            }
        }
    }

    /// <summary>
    /// Executes a task delegate asynchronously
    /// </summary>
    /// <param name="task">The task to execute</param>
    /// <returns>A task representing the execution</returns>
    private static async ValueTask ExecuteAsync(Delegate? task)
    {
        if (task is Func<Task> commitAsync)
            await commitAsync();
        if (task is Action commitSync)
            commitSync();
    }

    /// <summary>
    /// Record representing a stage with commit and optional rollback operations
    /// </summary>
    /// <param name="Commit">The commit operation</param>
    /// <param name="Rollback">The rollback operation</param>
    private readonly record struct StageInfo(Delegate Commit, Delegate? Rollback);
}
