using System;
using System.Threading;
using Annium.Logging;

namespace Annium.Net.WebSockets;

/// <summary>
/// Base class for WebSocket connection monitoring implementations.
/// </summary>
public abstract class ConnectionMonitorBase : IConnectionMonitor, ILogSubject
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
    /// Gets a value indicating whether the monitor is running, using a volatile read so background
    /// callers (e.g. timer callbacks) observe the latest write made by <see cref="Start"/> / <see cref="Stop"/>.
    /// </summary>
    protected bool IsRunning => Volatile.Read(ref _isRunning) == 1;

    /// <summary>
    /// Serializes <see cref="Start"/> / <see cref="Stop"/> so the corresponding
    /// <see cref="HandleStart"/> / <see cref="HandleStop"/> calls are mutually exclusive — a
    /// concurrent Stop can never tear down subclass state while Start is still building it.
    /// </summary>
    private readonly Lock _stateLock = new();

    /// <summary>
    /// Backing field for the running flag (1 = running, 0 = stopped). State transitions happen
    /// under <c>_stateLock</c>; background callers (e.g. timer callbacks) observe it via the
    /// volatile <see cref="IsRunning"/> read.
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

        lock (_stateLock)
        {
            if (Volatile.Read(ref _isRunning) == 1)
            {
                this.Trace("skip - already started");
                return;
            }

            // build subclass state first, then publish "running" — a Stop observing the flag is
            // therefore guaranteed to see a completed HandleStart (e.g. a non-null timer).
            HandleStart();
            Volatile.Write(ref _isRunning, 1);
        }

        this.Trace("done");
    }

    /// <summary>
    /// Stops the connection monitoring.
    /// </summary>
    public void Stop()
    {
        this.Trace("start");

        lock (_stateLock)
        {
            if (Volatile.Read(ref _isRunning) == 0)
            {
                this.Trace("skip - already stopped");
                return;
            }

            // clear "running" first so background callbacks bail out, then tear down subclass state.
            Volatile.Write(ref _isRunning, 0);
            HandleStop();
        }

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
