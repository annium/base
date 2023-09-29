using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Testing.Executors;

public class SetupExecutor : ITestExecutor, ILogSubject
{
    public ILogger Logger { get; }

    public uint Order => 2;

    public SetupExecutor(
        ILogger logger
    )
    {
        Logger = logger;
    }

    public Task ExecuteAsync(Target target)
    {
        this.Trace<string>("Setup {test}.", target.Test.DisplayName);

        target.Init();

        return Task.CompletedTask;
    }
}