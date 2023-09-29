using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging;

namespace Demo.Logging.Commands;

internal class ConsoleCommand : AsyncCommand, ICommandDescriptor, ILogSubject
{
    public static string Id => "console";
    public static string Description => $"test {Id} log handler";
    public ILogger Logger { get; }

    public ConsoleCommand(
        ILogger logger
    )
    {
        Logger = logger;
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        this.Debug("debug demo");
        await Task.Delay(100, ct);
        this.Trace("trace demo");
    }
}