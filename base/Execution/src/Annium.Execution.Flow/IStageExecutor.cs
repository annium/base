using System;
using System.Threading.Tasks;
using Annium.Data.Operations;

namespace Annium.Execution.Flow;

public interface IStageExecutor
{
    IStageExecutor Stage(Action commit);
    IStageExecutor Stage(Action commit, Action rollback);
    IStageExecutor Stage(Action commit, Func<Task> rollback);
    IStageExecutor Stage(Func<Task> commit);
    IStageExecutor Stage(Func<Task> commit, Action rollback);
    IStageExecutor Stage(Func<Task> commit, Func<Task> rollback);
    Task<IResult> RunAsync();
}
