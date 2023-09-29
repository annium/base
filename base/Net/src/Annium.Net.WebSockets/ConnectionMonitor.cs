using System;
using Annium.Logging;

namespace Annium.Net.WebSockets;

public class ConnectionMonitor : IConnectionMonitor, ILogSubject
{
    public static IConnectionMonitor None { get; } = new ConnectionMonitor();
    public ILogger Logger { get; } = VoidLogger.Instance;
    public event Action OnConnectionLost = delegate { };

    private ConnectionMonitor()
    {
    }

    public void Init(ISendingReceivingWebSocket socket)
    {
        this.Trace<string>("Init with {socket}", socket.GetFullId());
    }

    public void Start()
    {
        this.Trace("start");
    }

    public void Stop()
    {
        this.Trace("stop");
    }
}