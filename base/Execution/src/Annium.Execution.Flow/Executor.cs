using Annium.Execution.Flow.Internal;

namespace Annium.Execution.Flow;

/// <summary>
/// Factory class for creating flow executor instances
/// </summary>
public static class Executor
{
    /// <summary>
    /// Creates a new batch executor for executing multiple operations
    /// </summary>
    /// <returns>A new batch executor instance</returns>
    public static IBatchExecutor Batch() => new BatchExecutor();

    /// <summary>
    /// Creates a new stage executor for executing operations with commit/rollback support
    /// </summary>
    /// <returns>A new stage executor instance</returns>
    public static IStageExecutor Staged() => new StageExecutor();
}
