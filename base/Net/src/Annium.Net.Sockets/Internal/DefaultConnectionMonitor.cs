using System.Threading;
using Annium.Logging;

namespace Annium.Net.Sockets.Internal;

internal class DefaultConnectionMonitor : ConnectionMonitorBase
{
    private readonly ISendingReceivingSocket _socket;
    private CancellationTokenSource _cts = default!;

    public DefaultConnectionMonitor(ISendingReceivingSocket socket, ILogger logger)
        : base(logger)
    {
        _socket = socket;
    }

    protected override void HandleStart()
    {
        this.Trace("start");

        _cts = new CancellationTokenSource();

        this.Trace("done");
    }

    protected override void HandleStop()
    {
        this.Trace("start");

        _cts.Cancel();
        _cts.Dispose();

        this.Trace("done");
    }
}
