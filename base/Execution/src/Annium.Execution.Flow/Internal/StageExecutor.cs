using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Data.Operations;

namespace Annium.Execution.Flow.Internal;

internal class StageExecutor : IStageExecutor
{
    private readonly List<StageInfo> _stages = new();

    public IStageExecutor Stage(Action commit) => StageInternal(commit);

    public IStageExecutor Stage(Action commit, Action rollback) => StageInternal(commit, rollback);

    public IStageExecutor Stage(Action commit, Func<Task> rollback) => StageInternal(commit, rollback);

    public IStageExecutor Stage(Func<Task> commit) => StageInternal(commit);

    public IStageExecutor Stage(Func<Task> commit, Action rollback) => StageInternal(commit, rollback);

    public IStageExecutor Stage(Func<Task> commit, Func<Task> rollback) => StageInternal(commit, rollback);

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

    private StageExecutor StageInternal(Delegate commit, Delegate? rollback = null)
    {
        _stages.Add(new StageInfo(commit, rollback));

        return this;
    }

    private static async Task<int> CommitAsync(IEnumerable<StageInfo> stages, IResult result)
    {
        var i = 0;

        foreach (var stage in stages)
        {
            try
            {
                // count before stage run to include failed stage
                i++;

                await Execute(stage.Commit);
            }
            catch (Exception exception)
            {
                result.Error(exception.Message);
            }
        }

        return i;
    }

    private static async Task RollbackAsync(IEnumerable<StageInfo> stages, IResult result)
    {
        foreach (var stage in stages)
        {
            try
            {
                await Execute(stage.Rollback);
            }
            catch (Exception exception)
            {
                result.Error(exception.Message);
            }
        }
    }

    private static async ValueTask Execute(Delegate? task)
    {
        if (task is Func<Task> commitAsync)
            await commitAsync();
        if (task is Action commitSync)
            commitSync();
    }

    private readonly record struct StageInfo(Delegate Commit, Delegate? Rollback);
}
