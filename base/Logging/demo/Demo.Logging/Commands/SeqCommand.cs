using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Arguments;
using Annium.Logging;

namespace Demo.Logging.Commands;

internal class SeqCommand : AsyncCommand, ICommandDescriptor, ILogSubject
{
    public static string Id => "seq";
    public static string Description => $"test {Id} log handler";
    public ILogger Logger { get; }

    public SeqCommand(
        ILogger logger
    )
    {
        Logger = logger;
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        this.Info("demo");
        await Task.Delay(100, ct);
    }
}