using System;
using Annium.Execution.Background.Internal;
using Annium.Logging;

namespace Annium.Execution.Background;

/// <summary>
/// Factory class for creating executor instances
/// </summary>
public static class Executor
{
    /// <summary>
    /// Creates a parallel executor that runs tasks concurrently without limit
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <returns>A new parallel executor instance</returns>
    public static IExecutor Parallel<T>(ILogger logger) => new ParallelExecutor<T>(logger);

    /// <summary>
    /// Creates a concurrent executor that runs tasks with controlled parallelism
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="parallelism">The maximum number of concurrent tasks (0 for processor count)</param>
    /// <returns>A new concurrent executor instance</returns>
    public static IExecutor Concurrent<T>(ILogger logger, uint parallelism = 0) =>
        new ConcurrentExecutor<T>(parallelism == 0u ? Environment.ProcessorCount : (int)parallelism, logger);

    /// <summary>
    /// Creates a sequential executor that runs tasks one after another
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <returns>A new sequential executor instance</returns>
    public static IExecutor Sequential<T>(ILogger logger) => new SequentialExecutor<T>(logger);
}
