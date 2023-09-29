using System;
using System.Threading.Tasks;
using Annium.Data.Operations;

namespace Annium.Extensions.Execution;

public interface IBatchExecutor
{
    IBatchExecutor With(Action handler);
    IBatchExecutor With(Func<Task> handler);
    Task<IResult> RunAsync();
}