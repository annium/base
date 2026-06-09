using System;
using System.Threading;
using Annium.Logging;

namespace Annium.Net.Sockets;

/// <summary>
/// Base class for connection monitors that detect when socket connections are lost. Implements
/// <see cref="IConnectionMonitor"/> and centralizes the start/stop idempotency invariant
/// (<see cref="Interlocked"/>-guarded running flag); subclasses supply only
/// <see cref="HandleStart"/> / <see cref="HandleStop"/>.
/// </summary>
public abstract class ConnectionMonitorBase : IConnectionMonitor, ILogSubject
{
    /// <summary>
    /// Gets the logger instance.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Event raised when the connection is detected as lost.
    /// </summary>
    public event Action OnConnectionLost = delegate { };

    /// <summary>
    /// Backing field for the running flag (1 = running, 0 = stopped).
    /// Reads must use <see cref="Volatile"/>.Read or <see cref="Interlocked"/>.CompareExchange;
    /// writes go through Interlocked.CompareExchange in <see cref="Start"/> / <see cref="Stop"/>.
    /// </summary>
    private int _isRunning;

    /// <summary>
    /// Gets whether the monitor is currently running, using a volatile read so background callers
    /// (e.g. timer callbacks) observe the latest write made by <see cref="Start"/> / <see cref="Stop"/>.
    /// </summary>
    protected bool IsRunning => Volatile.Read(ref _isRunning) == 1;

    /// <summary>
    /// Initializes a new instance of the ConnectionMonitorBase class.
    /// </summary>
    /// <param name="logger">Logger instance for diagnostics.</param>
    protected ConnectionMonitorBase(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Starts the connection monitor.
    /// </summary>
    public void Start()
    {
        this.Trace("start");

        if (Interlocked.CompareExchange(ref _isRunning, 1, 0) == 1)
        {
            this.Trace("skip - already started");
            return;
        }

        HandleStart();

        this.Trace("done");
    }

    /// <summary>
    /// Stops the connection monitor.
    /// </summary>
    public void Stop()
    {
        this.Trace("start");

        if (Interlocked.CompareExchange(ref _isRunning, 0, 1) == 0)
        {
            this.Trace("skip - already stopped");
            return;
        }

        HandleStop();

        this.Trace("done");
    }

    /// <summary>
    /// Fires the connection lost event.
    /// </summary>
    protected void FireConnectionLost()
    {
        OnConnectionLost();
    }

    /// <summary>
    /// Handles the start logic for the specific monitor implementation.
    /// </summary>
    protected abstract void HandleStart();

    /// <summary>
    /// Handles the stop logic for the specific monitor implementation.
    /// </summary>
    protected abstract void HandleStop();
}
