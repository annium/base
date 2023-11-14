using System;
using Annium.Logging;

namespace Annium.Net.Sockets.Internal;

internal class DefaultConnectionMonitor : IConnectionMonitor, ILogSubject
{
    public ILogger Logger { get; }
    public event Action OnConnectionLost = delegate { };
    private readonly ISendingReceivingSocket _socket;

    public DefaultConnectionMonitor(ISendingReceivingSocket socket, ILogger logger)
    {
        Logger = logger;
        _socket = socket;
    }

    public void Start() { }

    public void Stop() { }
}
