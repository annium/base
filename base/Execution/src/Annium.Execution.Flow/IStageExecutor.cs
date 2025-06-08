using System;
using System.Threading.Tasks;
using Annium.Data.Operations;

namespace Annium.Execution.Flow;

/// <summary>
/// Interface for executing operations in stages with commit and rollback support
/// </summary>
public interface IStageExecutor
{
    /// <summary>
    /// Adds a stage with a synchronous commit operation
    /// </summary>
    /// <param name="commit">The commit operation</param>
    /// <returns>The stage executor for method chaining</returns>
    IStageExecutor Stage(Action commit);

    /// <summary>
    /// Adds a stage with synchronous commit and rollback operations
    /// </summary>
    /// <param name="commit">The commit operation</param>
    /// <param name="rollback">The rollback operation</param>
    /// <returns>The stage executor for method chaining</returns>
    IStageExecutor Stage(Action commit, Action rollback);

    /// <summary>
    /// Adds a stage with synchronous commit and asynchronous rollback operations
    /// </summary>
    /// <param name="commit">The commit operation</param>
    /// <param name="rollback">The rollback operation</param>
    /// <returns>The stage executor for method chaining</returns>
    IStageExecutor Stage(Action commit, Func<Task> rollback);

    /// <summary>
    /// Adds a stage with an asynchronous commit operation
    /// </summary>
    /// <param name="commit">The commit operation</param>
    /// <returns>The stage executor for method chaining</returns>
    IStageExecutor Stage(Func<Task> commit);

    /// <summary>
    /// Adds a stage with asynchronous commit and synchronous rollback operations
    /// </summary>
    /// <param name="commit">The commit operation</param>
    /// <param name="rollback">The rollback operation</param>
    /// <returns>The stage executor for method chaining</returns>
    IStageExecutor Stage(Func<Task> commit, Action rollback);

    /// <summary>
    /// Adds a stage with asynchronous commit and rollback operations
    /// </summary>
    /// <param name="commit">The commit operation</param>
    /// <param name="rollback">The rollback operation</param>
    /// <returns>The stage executor for method chaining</returns>
    IStageExecutor Stage(Func<Task> commit, Func<Task> rollback);

    /// <summary>
    /// Executes all stages and returns the result
    /// </summary>
    /// <returns>The result of the staged execution</returns>
    Task<IResult> RunAsync();
}
