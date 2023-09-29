using System;
using Annium.Extensions.Execution.Internal;
using Annium.Extensions.Execution.Internal.Background;
using Annium.Logging;

namespace Annium.Extensions.Execution;

public static class Executor
{
    public static IBatchExecutor Batch() => new BatchExecutor();
    public static IStageExecutor Staged() => new StageExecutor();

    public static class Background
    {
        public static IBackgroundExecutor Parallel<T>(ILogger logger) =>
            new ParallelBackgroundExecutor<T>(logger);

        public static IBackgroundExecutor Concurrent<T>(ILogger logger, uint parallelism = 0) =>
            new ConcurrentBackgroundExecutor<T>(parallelism == 0u ? Environment.ProcessorCount : (int)parallelism, logger);

        public static IBackgroundExecutor Sequential<T>(ILogger logger) =>
            new SequentialBackgroundExecutor<T>(logger);
    }
}