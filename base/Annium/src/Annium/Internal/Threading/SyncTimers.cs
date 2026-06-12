using System;
using System.Threading;
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
/// On <see cref="TimerBase.Dispose"/>, the underlying timer is drained via the
/// <see cref="System.Threading.Timer.Dispose(WaitHandle)"/> overload before returning, so an in-flight
/// <see cref="Handle"/> completes against still-live owner state. No <c>OnDrainCompleted</c> hook is
/// needed: the wait handle is signaled only after the last synchronous callback body returns. If the
/// drain exceeds <see cref="TimerConstants.DisposeWaitBudget"/>, the wait handle is intentionally leaked
/// and a warning is logged so the still-running callback can complete safely; callers MUST NOT free shared
/// state on timeout without independent synchronization.
/// </remarks>
internal abstract class SyncTimerBase : TimerBase, ISequentialTimer
{
    /// <summary>
    /// A flag indicating whether the timer is currently handling a callback (1) or not (0).
    /// Intentionally NOT consulted in <see cref="TimerBase.Dispose"/> —
    /// <see cref="System.Threading.Timer.Dispose(WaitHandle)"/> already drains synchronous callbacks fully
    /// (the wait handle is signaled only after the last callback body returns). This CAS is purely to
    /// prevent overlapping ticks during normal operation when the period is shorter than the callback
    /// duration and the runtime schedules a second callback on a different ThreadPool thread.
    /// </summary>
    private int _isHandling;

    /// <summary>
    /// Managed thread id of the thread currently executing <see cref="Handle"/>, or 0 if none. Used to
    /// detect re-entrant disposal from inside <see cref="Handle"/> and skip the drain to avoid
    /// <see cref="System.Threading.Timer.Dispose(WaitHandle)"/>'s documented self-deadlock when invoked
    /// from the timer's callback thread.
    /// </summary>
    private int _callbackThreadId;

    /// <summary>
    /// Initializes a new instance of the <see cref="SyncTimerBase"/> class with an inert timer; derived ctors
    /// MUST call <see cref="TimerBase.Start"/> as their last step to begin firing.
    /// </summary>
    /// <param name="logger">The logger instance for tracing operations.</param>
    protected SyncTimerBase(ILogger logger)
        : base(logger) { }

    /// <summary>
    /// Executes the timer's handler.
    /// </summary>
    protected abstract void Handle();

    /// <summary>
    /// Returns whether the current Dispose call is re-entrant (i.e. invoked from inside the callback's own thread).
    /// Re-entrant disposal skips the drain to avoid self-deadlock — the in-flight callback owns the timer.
    /// </summary>
    /// <returns><see langword="true"/> when Dispose was called from within the callback thread; otherwise <see langword="false"/>.</returns>
    protected override bool IsReentrantDispose() =>
        Volatile.Read(ref _callbackThreadId) == Environment.CurrentManagedThreadId;

    /// <summary>
    /// The callback invoked by the underlying timer. Runs <see cref="Handle"/> under the
    /// <see cref="_isHandling"/> CAS guard and traps exceptions so the timer keeps firing on subsequent ticks.
    /// </summary>
    /// <param name="state">The timer state object (unused).</param>
    protected override void InvokeCallback(object? state)
    {
        if (Interlocked.CompareExchange(ref _isHandling, 1, 0) == 1)
            return;

        Volatile.Write(ref _callbackThreadId, Environment.CurrentManagedThreadId);
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
            Volatile.Write(ref _callbackThreadId, 0);
            Interlocked.Exchange(ref _isHandling, 0);
        }
    }
}
