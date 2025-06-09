using Annium.Logging;

namespace Annium.Net.Sockets.Internal;

/// <summary>
/// Default factory implementation for creating connection monitors
/// </summary>
internal class DefaultConnectionMonitorFactory : IConnectionMonitorFactory
{
    /// <summary>
    /// The logger instance for creating monitors
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the DefaultConnectionMonitorFactory class
    /// </summary>
    /// <param name="logger">Logger instance for diagnostics</param>
    public DefaultConnectionMonitorFactory(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Creates a default connection monitor for the specified socket
    /// </summary>
    /// <param name="socket">The socket to monitor</param>
    /// <param name="options">Configuration options for the monitor</param>
    /// <returns>A new default connection monitor instance</returns>
    public ConnectionMonitorBase Create(ISendingReceivingSocket socket, ConnectionMonitorOptions options)
    {
        return new DefaultConnectionMonitor(socket, options, _logger);
    }
}
