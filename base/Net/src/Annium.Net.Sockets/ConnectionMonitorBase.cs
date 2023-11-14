using System;
using System.Threading;
using Annium.Logging;

namespace Annium.Net.Sockets;

public abstract class ConnectionMonitorBase : ILogSubject
{
    public ILogger Logger { get; }
    public event Action OnConnectionLost = delegate { };
    protected int IsRunning;

    protected ConnectionMonitorBase(ILogger logger)
    {
        Logger = logger;
    }

    public void Start()
    {
        this.Trace("start");

        if (Interlocked.CompareExchange(ref IsRunning, 1, 0) == 1)
        {
            this.Trace("skip - already started");
            return;
        }

        HandleStart();

        this.Trace("done");
    }

    public void Stop()
    {
        this.Trace("start");

        if (Interlocked.CompareExchange(ref IsRunning, 0, 1) == 0)
        {
            this.Trace("skip - already stopped");
            return;
        }

        HandleStop();

        this.Trace("done");
    }

    protected void FireConnectionLost()
    {
        OnConnectionLost();
    }

    protected abstract void HandleStart();
    protected abstract void HandleStop();
}
