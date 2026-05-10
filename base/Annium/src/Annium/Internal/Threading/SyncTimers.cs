using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Threading;

namespace Annium.Internal.Threading;

/// <summary>
/// Provides a synchronous timer that executes a handler with a state object at specified intervals.
/// </summary>
/// <typeparam name="T">The type of the state object.</typeparam>
internal sealed class SyncTimer<T> : SyncTimerBase
    where T : class
{
    /// <summary>
    /// The state object passed to the handler.
    /// </summary>
    private readonly T _state;

    /// <summary>
    /// The synchronous handler to execute.
    /// </summary>
    private readonly Action<T> _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="SyncTimer{T}"/> class.
    /// </summary>
    /// <param name="state">The state object to pass to the handler.</param>
    /// <param name="handler">The synchronous handler to execute.</param>
    /// <param name="dueTime">The amount of time to delay before the first execution.</param>
    /// <param name="period">The time interval between executions.</param>
    /// <param name="logger">The logger instance for tracing operations.</param>
    public SyncTimer(T state, Action<T> handler, int dueTime, int period, ILogger logger)
        : base(logger)
    {
        _state = state;
        _handler = handler;
        Start(dueTime, period);
    }

    /// <summary>
    /// Executes the handler with the state object.
    /// </summary>
    protected override void Handle()
    {
        _handler(_state);
    }
}

/// <summary>
/// Provides a synchronous timer that executes a handler at specified intervals.
/// </summary>
internal sealed class SyncTimer : SyncTimerBase
{
    /// <summary>
    /// The synchronous handler to execute.
    /// </summary>
    private readonly Action _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="SyncTimer"/> class.
    /// </summary>
    /// <param name="handler">The synchronous handler to execute.</param>
    /// <param name="dueTime">The amount of time to delay before the first execution.</param>
    /// <param name="period">The time interval between executions.</param>
    /// <param name="logger">The logger instance for tracing operations.</param>
    public SyncTimer(Action handler, int dueTime, int period, ILogger logger)
        : base(logger)
    {
        _handler = handler;
        Start(dueTime, period);
    }

    /// <summary>
    /// Executes the handler.
    /// </summary>
    protected override void Handle()
    {
        _handler();
    }
}

/// <summary>
/// Provides a base class for synchronous timers.
/// </summary>
/// <remarks>
/// On <see cref="Dispose"/>, the underlying <see cref="Timer"/> is drained via the
/// <see cref="Timer.Dispose(WaitHandle)"/> overload before returning, so an in-flight
/// <see cref="Handle"/> completes against still-live owner state. If the drain exceeds
/// <see cref="_disposeWaitBudget"/>, the wait handle is intentionally leaked and a warning is logged
/// so the still-running callback can complete safely; callers MUST NOT free shared state on timeout
/// without independent synchronization.
/// </remarks>
internal abstract class SyncTimerBase : ISequentialTimer, ILogSubject
{
    /// <summary>
    /// Maximum time <see cref="Dispose"/> waits for queued callbacks to drain before returning.
    /// </summary>
    private static readonly TimeSpan _disposeWaitBudget = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Gets the logger instance for tracing operations.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The underlying timer instance. Created in the base ctor with <see cref="Timeout.Infinite"/> and started by
    /// the derived ctor's call to <see cref="Start"/> after derived fields are assigned.
    /// </summary>
    protected Timer Timer { get; }

    /// <summary>
    /// A flag indicating whether the timer is currently handling a callback (1) or not (0).
    /// Intentionally NOT consulted in <see cref="Dispose"/> — <see cref="Timer.Dispose(WaitHandle)"/>
    /// already drains synchronous callbacks fully (the wait handle is signaled only after the last
    /// callback body returns). This CAS is purely to prevent overlapping ticks during normal operation.
    /// </summary>
    private int _isHandling;

    /// <summary>
    /// Managed thread id of the thread currently executing <see cref="Handle"/>, or 0 if none. Used by
    /// <see cref="Dispose"/> to detect re-entrant disposal from inside <see cref="Handle"/> and skip the
    /// drain to avoid <see cref="Timer.Dispose(WaitHandle)"/>'s documented self-deadlock when invoked from
    /// the timer's callback thread.
    /// </summary>
    private volatile int _callbackThreadId;

    /// <summary>
    /// 0 if <see cref="Dispose"/> has not yet claimed the dispose path; 1 once it has. Prevents a
    /// concurrent second caller from racing into <see cref="Timer.Dispose(WaitHandle)"/> on the
    /// already-disposed timer.
    /// </summary>
    private int _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="SyncTimerBase"/> class with an inert timer; derived ctors
    /// MUST call <see cref="Start"/> as their last step to begin firing.
    /// </summary>
    /// <param name="logger">The logger instance for tracing operations.</param>
    protected SyncTimerBase(ILogger logger)
    {
        Logger = logger;
        Timer = new Timer(Callback, null, Timeout.Infinite, Timeout.Infinite);
    }

    /// <summary>
    /// Begins firing the timer with the specified due time and period. Called by derived ctors after
    /// derived fields are assigned so callbacks observe a fully-initialized instance.
    /// </summary>
    /// <param name="dueTime">The amount of time to delay before the first execution.</param>
    /// <param name="period">The time interval between executions.</param>
    protected void Start(int dueTime, int period)
    {
        Timer.Change(dueTime, period);
    }

    /// <summary>
    /// Releases all resources used by the timer.
    /// </summary>
    /// <remarks>
    /// Drains queued ThreadPool callbacks via <see cref="Timer.Dispose(WaitHandle)"/> and waits for the
    /// drain (bounded by <see cref="_disposeWaitBudget"/>); on timeout the wait handle is leaked and a
    /// warning is logged so the still-running callback can complete without raising
    /// <see cref="ObjectDisposedException"/>. Re-entrant disposal — calling <see cref="Dispose"/> from
    /// inside <see cref="Handle"/> on the same thread — is detected via <see cref="_callbackThreadId"/>
    /// and skips the drain (which would otherwise self-deadlock per <see cref="Timer.Dispose(WaitHandle)"/>'s
    /// documented behavior); the timer is still stopped from issuing new callbacks.
    /// </remarks>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
            return;

        if (_callbackThreadId == Environment.CurrentManagedThreadId)
        {
            // Re-entrant dispose from within Handle on the timer's callback thread. Stop the timer; do
            // NOT block on Timer.Dispose(WaitHandle) — it would deadlock waiting for the very callback
            // that is currently executing this Dispose call.
            Timer.Dispose();
            return;
        }

        var drained = new ManualResetEvent(false);
        Timer.Dispose(drained);

        if (drained.WaitOne(_disposeWaitBudget))
        {
            drained.Dispose();
            return;
        }

        this.Warn(
            "Timer drain exceeded {budget} budget; wait handle intentionally leaked to allow queued callbacks to complete",
            _disposeWaitBudget
        );
    }

    /// <summary>
    /// Asynchronously releases all resources used by the timer. The drain is currently synchronous; this
    /// method exists to satisfy <see cref="IAsyncDisposable"/> for callers that prefer <c>await using</c>.
    /// </summary>
    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Changes the start time and the interval between method invocations for a timer.
    /// </summary>
    /// <param name="dueTime">The amount of time to delay before the first execution.</param>
    /// <param name="period">The time interval between executions.</param>
    /// <returns>true if the timer was successfully updated; otherwise, false.</returns>
    public bool Change(int dueTime, int period)
    {
        return Timer.Change(dueTime, period);
    }

    /// <summary>
    /// Changes the start time and the interval between method invocations for a timer.
    /// </summary>
    /// <param name="dueTime">The amount of time to delay before the first execution.</param>
    /// <param name="period">The time interval between executions.</param>
    /// <returns>true if the timer was successfully updated; otherwise, false.</returns>
    public bool Change(TimeSpan dueTime, TimeSpan period)
    {
        return Timer.Change(dueTime, period);
    }

    /// <summary>
    /// Executes the timer's handler.
    /// </summary>
    protected abstract void Handle();

    /// <summary>
    /// The callback method that is called when the timer elapses.
    /// </summary>
    /// <param name="_">The state object passed to the timer (unused).</param>
    private void Callback(object? _)
    {
        if (Interlocked.CompareExchange(ref _isHandling, 1, 0) == 1)
            return;

        _callbackThreadId = Environment.CurrentManagedThreadId;
        try
        {
            Handle();
        }
        catch (Exception e)
        {
            this.Error(e);
        }
        finally
        {
            _callbackThreadId = 0;
            Interlocked.Exchange(ref _isHandling, 0);
        }
    }
}
