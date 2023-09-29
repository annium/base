using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Testing.Executors;

public class AfterExecutor : ITestExecutor, ILogSubject
{
    public uint Order => 5;
    public ILogger Logger { get; }
    private readonly MethodExecutor _executor;

    public AfterExecutor(
        MethodExecutor executor,
        ILogger logger
    )
    {
        _executor = executor;
        Logger = logger;
    }

    public Task ExecuteAsync(Target target)
    {
        var (instance, test, result) = target;
        if (test.After == null)
            return Task.CompletedTask;

        this.Trace<string>("Execute After of {test}.", target.Test.DisplayName);

        return _executor.ExecuteAsync(instance, test.After, result);
    }
}