using Annium.Logging;

namespace Annium.Net.Sockets.Internal;

internal class DefaultConnectionMonitorFactory : IConnectionMonitorFactory
{
    private readonly ILogger _logger;

    public DefaultConnectionMonitorFactory(ILogger logger)
    {
        _logger = logger;
    }

    public ConnectionMonitorBase Create(ISendingReceivingSocket socket, ConnectionMonitorOptions options)
    {
        return new DefaultConnectionMonitor(socket, options, _logger);
    }
}
