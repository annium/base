using Annium.Logging;

namespace Annium.Net.WebSockets.Internal;

internal class NoneConnectionMonitor : ConnectionMonitorBase
{
    public NoneConnectionMonitor(ILogger logger)
        : base(logger) { }

    protected override void HandleStart()
    {
        this.Trace("done");
    }

    protected override void HandleStop()
    {
        this.Trace("done");
    }
}
