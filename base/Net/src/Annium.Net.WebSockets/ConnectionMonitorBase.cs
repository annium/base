using System;
using System.Threading;
using Annium.Logging;

namespace Annium.Net.WebSockets;

/// <summary>
/// Base class for WebSocket connection monitoring implementations.
/// </summary>
public abstract class ConnectionMonitorBase : ILogSubject
{
    /// <summary>
    /// Gets the logger instance for this connection monitor.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Event triggered when a connection loss is detected.
    /// </summary>
    public event Action OnConnectionLost = delegate { };

    /// <summary>
    /// Returns the current running flag using a volatile read so background callers (e.g. timer
    /// callbacks) observe the latest write made by <see cref="Start"/> / <see cref="Stop"/>.
    /// </summary>
    /// <returns>1 when the monitor is running, 0 otherwise.</returns>
    protected int ReadIsRunning() => Volatile.Read(ref _isRunning);

    /// <summary>
    /// Backing field for the running flag (1 = running, 0 = stopped).
    /// Reads must use <see cref="Volatile"/>.Read or <see cref="Interlocked"/>.CompareExchange;
    /// writes go through Interlocked.CompareExchange in <see cref="Start"/> / <see cref="Stop"/>.
    /// </summary>
    private int _isRunning;

    /// <summary>
    /// Initializes a new instance of the ConnectionMonitorBase class.
    /// </summary>
    /// <param name="logger">Logger instance for tracing and error reporting.</param>
    protected ConnectionMonitorBase(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Starts the connection monitoring.
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
    /// Stops the connection monitoring.
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
    /// Triggers the connection lost event.
    /// </summary>
    protected void FireConnectionLost()
    {
        OnConnectionLost();
    }

    /// <summary>
    /// When overridden in a derived class, handles the start operation.
    /// </summary>
    protected abstract void HandleStart();

    /// <summary>
    /// When overridden in a derived class, handles the stop operation.
    /// </summary>
    protected abstract void HandleStop();
}
