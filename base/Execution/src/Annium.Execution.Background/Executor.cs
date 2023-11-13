using System;
using Annium.Execution.Background.Internal;
using Annium.Logging;

namespace Annium.Execution.Background;

public static class Executor
{
    public static IExecutor Parallel<T>(ILogger logger) => new ParallelExecutor<T>(logger);

    public static IExecutor Concurrent<T>(ILogger logger, uint parallelism = 0) =>
        new ConcurrentExecutor<T>(parallelism == 0u ? Environment.ProcessorCount : (int)parallelism, logger);

    public static IExecutor Sequential<T>(ILogger logger) => new SequentialExecutor<T>(logger);
}
