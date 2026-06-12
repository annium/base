using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Threading;

namespace Annium.Internal.Threading;

/// <summary>
/// Common scaffolding for the three timer base classes (<see cref="AsyncTimerBase"/>,
/// <see cref="SyncTimerBase"/>, <see cref="DebounceTimerBase"/>): owns the underlying
/// <see cref="Timer"/>, the idempotent dispose gate, the <see cref="Timer.Dispose(WaitHandle)"/> drain,
/// and the leak-on-timeout warning. Per-type concerns (re-entrant detection mechanism,
/// optional in-flight gate to drain after the WaitHandle, public surface) are implemented by
/// derived classes via the <see cref="IsReentrantDispose"/> and <see cref="OnDrainCompleted"/> hooks.
/// </summary>
internal abstract class TimerBase : ILogSubject
{
    /// <summary>
    /// Gets the logger instance for tracing operations.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The underlying timer. Created in the base ctor with <see cref="Timeout.Infinite"/> so callbacks
    /// don't fire until the derived ctor calls <see cref="Start"/> (or, for debounce, until
    /// <c>Request</c> arms the timer for the first time).
    /// </summary>
    private readonly Timer _timer;

    /// <summary>
    /// 0 if <see cref="Dispose"/> has not yet claimed the dispose path; 1 once it has. Set BEFORE the
    /// drain begins so concurrent operations can observe disposal and short-circuit.
    /// </summary>
    private int _disposed;

    /// <summary>
    /// Whether <see cref="Dispose"/> has claimed the dispose path. Uses <c>Volatile.Read</c> so cross-thread
    /// observers see the disposal flag in a timely fashion on weakly-ordered architectures
    /// (e.g. ARM64). Derived classes use this for a fast-path short-circuit before performing observable
    /// side effects (e.g. arming the timer); the underlying <see cref="System.Threading.Timer"/> may still
    /// race with its own <c>Dispose(WaitHandle)</c>, so callers MUST guard timer mutations with
    /// <c>try/catch (ObjectDisposedException)</c> as well.
    /// </summary>
    protected bool IsDisposed => Volatile.Read(ref _disposed) != 0;

    /// <summary>
    /// Initializes the base with an inert timer and a logger. Derived ctors MUST call
    /// <see cref="Start"/> as their last step (or, for debounce-style timers, simply not start).
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    protected TimerBase(ILogger logger)
    {
        Logger = logger;
        _timer = new Timer(InvokeCallback, null, Timeout.Infinite, Timeout.Infinite);
    }

    /// <summary>
    /// Begins firing the timer with the specified due time and period. Called by derived ctors after
    /// derived fields are assigned so callbacks observe a fully-initialized instance.
    /// </summary>
    /// <param name="dueTime">The amount of time to delay before the first execution.</param>
    /// <param name="period">The time interval between executions.</param>
    protected void Start(int dueTime, int period)
    {
        _timer.Change(dueTime, period);
    }

    /// <summary>
    /// Changes the due time and period of the underlying timer. Public surface for derived classes that
    /// implement <see cref="ISequentialTimer"/>; the inherited methods satisfy the interface contract.
    /// <see cref="DebounceTimerBase"/> inherits these methods too but exposes them as bypass-the-debounce
    /// escape hatches — callers that arm a debounce timer directly via <c>Change(dueTime, period)</c> are
    /// deliberately stepping outside the debounce protocol.
    /// </summary>
    /// <param name="dueTime">The amount of time to delay before the next execution.</param>
    /// <param name="period">The time interval between executions.</param>
    /// <returns>true if the timer was successfully updated; otherwise, false.</returns>
    public bool Change(int dueTime, int period)
    {
        return _timer.Change(dueTime, period);
    }

    /// <summary>
    /// Changes the due time and period of the underlying timer.
    /// </summary>
    /// <param name="dueTime">The amount of time to delay before the next execution.</param>
    /// <param name="period">The time interval between executions.</param>
    /// <returns>true if the timer was successfully updated; otherwise, false.</returns>
    public bool Change(TimeSpan dueTime, TimeSpan period)
    {
        return _timer.Change(dueTime, period);
    }

    /// <summary>
    /// Releases all resources used by the timer.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The dispose path is idempotent: a second concurrent caller observes <c>_disposed == 1</c> and
    /// returns immediately. Re-entrant disposal (via <see cref="IsReentrantDispose"/>) skips the drain
    /// to avoid the self-deadlock that would otherwise occur when <see cref="Timer.Dispose(WaitHandle)"/>
    /// is invoked from inside the timer's own callback.
    /// </para>
    /// <para>
    /// Otherwise the underlying <see cref="Timer"/> is drained via the
    /// <see cref="Timer.Dispose(WaitHandle)"/> overload, then the derived class's <see cref="OnDrainCompleted"/>
    /// hook runs (typically to acquire and dispose an in-flight callback gate). On either timeout, the wait
    /// handle (and any per-derived state) is intentionally leaked and a warning is logged so the
    /// still-running callback can complete without raising <see cref="ObjectDisposedException"/>; callers
    /// MUST NOT free shared state on timeout without independent synchronization.
    /// </para>
    /// </remarks>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
            return;

        if (IsReentrantDispose())
        {
            // Re-entrant dispose from inside the running callback: blocking on Timer.Dispose(WaitHandle)
            // would deadlock waiting for the very callback that called us. Stop the timer and return;
            // the derived class is responsible for any per-instance gate cleanup (typically deferred
            // to the running callback's finally block).
            _timer.Dispose();
            return;
        }

        var drained = new ManualResetEvent(false);
        _timer.Dispose(drained);

        if (!drained.WaitOne(TimerConstants.DisposeWaitBudget))
        {
            // Drain timed out: queued ThreadPool callbacks may still execute. Leak the wait handle so
            // the late Set() from the drain pipeline is harmless.
            this.Warn(
                "Timer drain exceeded {budget} budget; wait handle intentionally leaked to allow queued callbacks to complete",
                TimerConstants.DisposeWaitBudget
            );
            return;
        }

        drained.Dispose();

        // Derived classes drain their own in-flight callback state (e.g. SemaphoreSlim gate for async
        // timers). For synchronous timers there is no work to do here.
        OnDrainCompleted();
    }

    /// <summary>
    /// Asynchronously releases all resources used by the timer. The drain is currently synchronous; this
    /// method exists to satisfy <see cref="IAsyncDisposable"/> for callers that prefer <c>await using</c>.
    /// </summary>
    /// <returns>A completed <see cref="ValueTask"/>.</returns>
    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Whether the current call to <see cref="Dispose"/> is being made from inside the timer's own
    /// callback execution context. When true, the base skips the WaitHandle drain and the
    /// <see cref="OnDrainCompleted"/> hook to avoid self-deadlock.
    /// </summary>
    /// <returns>true if re-entrant; otherwise false.</returns>
    protected abstract bool IsReentrantDispose();

    /// <summary>
    /// Hook invoked by <see cref="Dispose"/> after the WaitHandle drain has succeeded. Used by derived
    /// classes that own an additional in-flight gate (e.g. <see cref="SemaphoreSlim"/>) to acquire and
    /// dispose it. The default implementation is a no-op (sufficient for synchronous timers, where
    /// <see cref="Timer.Dispose(WaitHandle)"/> drains synchronous callbacks fully).
    /// </summary>
    protected virtual void OnDrainCompleted() { }

    /// <summary>
    /// The callback invoked by the underlying <see cref="Timer"/>. Derived classes implement the
    /// per-type callback semantics (sync vs. async, debounce coordination, etc.).
    /// </summary>
    /// <param name="state">The timer state object (always null in this codebase).</param>
    protected abstract void InvokeCallback(object? state);
}
