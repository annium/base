using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Data.Operations;

namespace Annium.Execution.Flow.Internal;

/// <summary>
/// Internal implementation of batch executor for running multiple operations
/// </summary>
internal class BatchExecutor : IBatchExecutor
{
    /// <summary>
    /// List of handlers to execute in the batch
    /// </summary>
    private readonly IList<Delegate> _handlers = new List<Delegate>();

    /// <summary>
    /// Adds a synchronous operation to the batch
    /// </summary>
    /// <param name="handler">The operation to add</param>
    /// <returns>The batch executor for method chaining</returns>
    public IBatchExecutor With(Action handler) => WithInternal(handler);

    /// <summary>
    /// Adds an asynchronous operation to the batch
    /// </summary>
    /// <param name="handler">The operation to add</param>
    /// <returns>The batch executor for method chaining</returns>
    public IBatchExecutor With(Func<Task> handler) => WithInternal(handler);

    /// <summary>
    /// Internal method for adding handlers to the batch
    /// </summary>
    /// <param name="handler">The handler to add</param>
    /// <returns>The batch executor instance</returns>
    private BatchExecutor WithInternal(Delegate handler)
    {
        _handlers.Add(handler);

        return this;
    }

    /// <summary>
    /// Executes all operations in the batch and returns the result
    /// </summary>
    /// <returns>The result of the batch execution</returns>
    public async Task<IResult> RunAsync()
    {
        var result = Result.New();

        // run each stage
        foreach (var handler in _handlers)
        {
            try
            {
                if (handler is Func<Task> handleAsync)
                    await handleAsync();
                if (handler is Action handleSync)
                    handleSync();
            }
            catch (Exception exception)
            {
                result.Error(exception.Message);
            }
        }

        return result;
    }
}
