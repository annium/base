using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing.Elements;

namespace Annium.Testing.Executors;

public class PipelineExecutor : ILogSubject
{
    public ILogger Logger { get; }
    private readonly ITestExecutor[] _executors;

    public PipelineExecutor(
        IEnumerable<ITestExecutor> executors,
        ILogger logger
    )
    {
        _executors = executors.OrderBy(e => e.Order).ToArray();
        Logger = logger;
    }

    public async Task ExecuteAsync(Target target)
    {
        this.Trace<string>("Start pipeline of {test}.", target.Test.DisplayName);

        var result = target.Result;
        result.ExecutionStart = DateTime.Now;

        foreach (var executor in _executors)
        {
            await executor.ExecuteAsync(target);
            if (result.Outcome != TestOutcome.None)
                break;
        }

        if (result.Outcome == TestOutcome.None)
            result.Outcome = TestOutcome.Passed;

        result.ExecutionEnd = DateTime.Now;

        this.Trace("Finished pipeline of {test} with {outcome}.", target.Test.DisplayName, result.Outcome);
    }
}