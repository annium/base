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
    public IStageExecutor Stage(Action commit, Func<ValueTask> rollback) => StageInternal(commit, rollback);

    /// <summary>
    /// Adds a stage with an asynchronous commit operation
    /// </summary>
    /// <param name="commit">The commit operation</param>
    /// <returns>The stage executor for method chaining</returns>
    public IStageExecutor Stage(Func<ValueTask> commit) => StageInternal(commit);

    /// <summary>
    /// Adds a stage with asynchronous commit and synchronous rollback operations
    /// </summary>
    /// <param name="commit">The commit operation</param>
    /// <param name="rollback">The rollback operation</param>
    /// <returns>The stage executor for method chaining</returns>
    public IStageExecutor Stage(Func<ValueTask> commit, Action rollback) => StageInternal(commit, rollback);

    /// <summary>
    /// Adds a stage with asynchronous commit and rollback operations
    /// </summary>
    /// <param name="commit">The commit operation</param>
    /// <param name="rollback">The rollback operation</param>
    /// <returns>The stage executor for method chaining</returns>
    public IStageExecutor Stage(Func<ValueTask> commit, Func<ValueTask> rollback) => StageInternal(commit, rollback);

    /// <summary>
    /// Executes all stages and returns the result
    /// </summary>
    /// <returns>The result of the staged execution</returns>
    public async Task<IResult> RunAsync()
    {
        var result = Result.Create();
        var executedStages = await CommitAsync(_stages, result);

        // if no exceptions - done
        if (result.IsOk)
            return result;

        // exception caught, rollback committed stages in reverse (LIFO) order
        await RollbackAsync(_stages.Take(executedStages).Reverse(), result);

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
    /// Commits stages in order and returns the number that completed successfully. On the first
    /// commit failure the loop stops — subsequent stages are neither committed nor counted. The
    /// returned count is the number of stages the caller must roll back.
    /// </summary>
    /// <param name="stages">The stages to commit</param>
    /// <param name="result">The result to store errors in</param>
    /// <returns>The number of stages that committed successfully</returns>
    private static async ValueTask<int> CommitAsync(IEnumerable<StageInfo> stages, IResult result)
    {
        var i = 0;

        foreach (var stage in stages)
        {
            try
            {
                await FlowHelper.ExecuteAsync(stage.Commit);
                i++;
            }
            catch (Exception exception)
            {
                result.Error(exception.Message);
                return i;
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
    private static async ValueTask RollbackAsync(IEnumerable<StageInfo> stages, IResult result)
    {
        foreach (var stage in stages)
        {
            try
            {
                await FlowHelper.ExecuteAsync(stage.Rollback);
            }
            catch (Exception exception)
            {
                result.Error(exception.Message);
            }
        }
    }

    /// <summary>
    /// Record representing a stage with commit and optional rollback operations
    /// </summary>
    /// <param name="Commit">The commit operation</param>
    /// <param name="Rollback">The rollback operation</param>
    private readonly record struct StageInfo(Delegate Commit, Delegate? Rollback);
}
