using Annium.Logging;

namespace Annium.Net.WebSockets.Internal;

/// <summary>
/// Default factory implementation for creating connection monitors
/// </summary>
internal class DefaultConnectionMonitorFactory : IConnectionMonitorFactory
{
    /// <summary>
    /// Logger instance to be passed to created monitors
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the DefaultConnectionMonitorFactory class
    /// </summary>
    /// <param name="logger">Logger instance for created monitors</param>
    public DefaultConnectionMonitorFactory(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Creates a new connection monitor for the specified WebSocket
    /// </summary>
    /// <param name="socket">The WebSocket to monitor</param>
    /// <param name="options">Configuration options for the monitor</param>
    /// <returns>A new DefaultConnectionMonitor instance</returns>
    public ConnectionMonitorBase Create(ISendingReceivingWebSocket socket, ConnectionMonitorOptions options)
    {
        return new DefaultConnectionMonitor(socket, options, _logger);
    }
}
