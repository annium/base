using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging;

namespace Demo.Logging.Commands;

internal class InMemoryCommand : AsyncCommand, ICommandDescriptor, ILogSubject
{
    public static string Id => "in-memory";
    public static string Description => $"test {Id} log handler";
    public ILogger Logger { get; }

    public InMemoryCommand(
        ILogger logger
    )
    {
        Logger = logger;
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        this.Trace("demo");
        await Task.Delay(100, ct);
    }
}