using Annium.Execution.Flow.Internal;

namespace Annium.Execution.Flow;

public static class Executor
{
    public static IBatchExecutor Batch() => new BatchExecutor();

    public static IStageExecutor Staged() => new StageExecutor();
}
