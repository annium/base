using System;
using System.Threading;
using Annium.Logging;

namespace Annium.Net.Sockets;

/// <summary>
/// Base class for connection monitors that detect when socket connections are lost. Implements
/// <see cref="IConnectionMonitor"/> and centralizes the start/stop idempotency invariant; subclasses
/// supply only <see cref="HandleStart"/> / <see cref="HandleStop"/>. The transition is serialized
/// under <c>_stateLock</c> so <see cref="HandleStart"/> and <see cref="HandleStop"/> never overlap,
/// and the running flag flips to 1 only after <see cref="HandleStart"/> has fully completed — so a
/// <see cref="Stop"/> that observes "running" is guaranteed to see fully-initialized subclass state.
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
    /// Stops the connection monitor.
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
