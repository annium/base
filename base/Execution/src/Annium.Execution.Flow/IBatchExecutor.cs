using System;
using System.Threading.Tasks;
using Annium.Data.Operations;

namespace Annium.Execution.Flow;

/// <summary>
/// Interface for executing multiple operations as a batch with error handling
/// </summary>
public interface IBatchExecutor
{
    /// <summary>
    /// Adds a synchronous operation to the batch
    /// </summary>
    /// <param name="handler">The operation to add</param>
    /// <returns>The batch executor for method chaining</returns>
    IBatchExecutor With(Action handler);

    /// <summary>
    /// Adds an asynchronous operation to the batch
    /// </summary>
    /// <param name="handler">The operation to add</param>
    /// <returns>The batch executor for method chaining</returns>
    IBatchExecutor With(Func<Task> handler);

    /// <summary>
    /// Executes all operations in the batch and returns the result
    /// </summary>
    /// <returns>The result of the batch execution</returns>
    Task<IResult> RunAsync();
}
