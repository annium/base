using System.Threading.Tasks;
using Annium.Logging;
using Annium.Testing.Elements;

namespace Annium.Testing.Executors;

public class SkippedExecutor : ITestExecutor, ILogSubject
{
    public ILogger Logger { get; }

    public uint Order => 1;

    public SkippedExecutor(
        ILogger logger
    )
    {
        Logger = logger;
    }

    public Task ExecuteAsync(Target target)
    {
        if (target.Test.IsSkipped)
        {
            target.Result.Outcome = TestOutcome.Skipped;

            this.Trace<string>("Skip {test}.", target.Test.DisplayName);
        }

        return Task.CompletedTask;
    }
}