using Annium.Logging;

namespace Annium.Net.Sockets.Internal;

/// <summary>
/// A no-operation connection monitor that performs no actual monitoring
/// </summary>
internal class NoneConnectionMonitor : ConnectionMonitorBase
{
    /// <summary>
    /// Initializes a new instance of the NoneConnectionMonitor class
    /// </summary>
    /// <param name="logger">Logger instance for diagnostics</param>
    public NoneConnectionMonitor(ILogger logger)
        : base(logger) { }

    /// <summary>
    /// Handles start operation (no-op)
    /// </summary>
    protected override void HandleStart()
    {
        this.Trace("done");
    }

    /// <summary>
    /// Handles stop operation (no-op)
    /// </summary>
    protected override void HandleStop()
    {
        this.Trace("done");
    }
}
