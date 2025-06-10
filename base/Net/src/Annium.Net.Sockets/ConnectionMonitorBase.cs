using System;
using System.Threading;
using Annium.Logging;

namespace Annium.Net.Sockets;

/// <summary>
/// Base class for connection monitors that detect when socket connections are lost
/// </summary>
public abstract class ConnectionMonitorBase : ILogSubject
{
    /// <summary>
    /// Gets the logger instance
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Event raised when the connection is detected as lost
    /// </summary>
    public event Action OnConnectionLost = delegate { };

    /// <summary>
    /// Indicates whether the monitor is currently running (1) or stopped (0)
    /// </summary>
    protected int IsRunning;

    /// <summary>
    /// Initializes a new instance of the ConnectionMonitorBase class
    /// </summary>
    /// <param name="logger">Logger instance for diagnostics</param>
    protected ConnectionMonitorBase(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Starts the connection monitor
    /// </summary>
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

    /// <summary>
    /// Stops the connection monitor
    /// </summary>
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

    /// <summary>
    /// Fires the connection lost event
    /// </summary>
    protected void FireConnectionLost()
    {
        OnConnectionLost();
    }

    /// <summary>
    /// Handles the start logic for the specific monitor implementation
    /// </summary>
    protected abstract void HandleStart();

    /// <summary>
    /// Handles the stop logic for the specific monitor implementation
    /// </summary>
    protected abstract void HandleStop();
}
