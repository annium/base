using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Threading;

namespace Annium.Internal.Threading;

/// <summary>
/// Provides a debounced timer that executes a handler with a state object after a period of inactivity.
/// </summary>
/// <typeparam name="T">The type of the state object.</typeparam>
internal sealed class DebounceTimer<T> : DebounceTimerBase
{
    /// <summary>
    /// The state object passed to the handler.
    /// </summary>
    private readonly T _state;

    /// <summary>
    /// The asynchronous handler to execute.
    /// </summary>
    private readonly Func<T, ValueTask> _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="DebounceTimer{T}"/> class.
    /// </summary>
    /// <param name="state">The state object to pass to the handler.</param>
    /// <param name="handler">The asynchronous handler to execute.</param>
    /// <param name="period">The time interval to wait before executing the handler.</param>
    /// <param name="logger">The logger instance for tracing operations.</param>
    public DebounceTimer(T state, Func<T, ValueTask> handler, int period, ILogger logger)
        : base(period, logger)
    {
        _state = state;
        _handler = handler;
    }

    /// <summary>
    /// Executes the handler with the state object.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override ValueTask HandleAsync()
    {
        return _handler(_state);
    }
}

/// <summary>
/// Provides a debounced timer that executes a handler after a period of inactivity.
/// </summary>
internal sealed class DebounceTimer : DebounceTimerBase
{
    /// <summary>
    /// The asynchronous handler to execute.
    /// </summary>
    private readonly Func<ValueTask> _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="DebounceTimer"/> class.
    /// </summary>
    /// <param name="handler">The asynchronous handler to execute.</param>
    /// <param name="period">The time interval to wait before executing the handler.</param>
    /// <param name="logger">The logger instance for tracing operations.</param>
    public DebounceTimer(Func<ValueTask> handler, int period, ILogger logger)
        : base(period, logger)
    {
        _handler = handler;
    }

    /// <summary>
    /// Executes the handler.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override ValueTask HandleAsync()
    {
        return _handler();
    }
}

/// <summary>
/// Base class for debounced timers. The gate / re-entrant-dispose / async-void dispatch machinery lives
/// in <see cref="AsyncTimerGateBase"/>; this class adds the debounce-specific state (<see cref="_period"/>,
/// <see cref="_isRequested"/>), the <see cref="Request"/> entry point, and the per-callback hooks that
/// claim the pending request inside the gate and re-fire the timer if a fresh request arrived while the
/// handler was running.
/// </summary>
internal abstract class DebounceTimerBase : AsyncTimerGateBase, IDebounceTimer
{
    /// <summary>
    /// The time interval to wait before executing the handler. Volatile so cross-thread reads in
    /// <see cref="Request"/> observe writes from <see cref="Change(int)"/> without a stale value
    /// on weakly-ordered architectures.
    /// </summary>
    private volatile int _period;

    /// <summary>
    /// A flag indicating whether a new request has been made (1) or not (0). Accessed exclusively via
    /// <see cref="Interlocked"/> operations whose full memory barriers provide the cross-thread ordering
    /// guarantees the volatile keyword would otherwise add — keeping the field plain matches the
    /// <c>_isHandling</c> convention in <see cref="SyncTimerBase"/>.
    /// </summary>
    private int _isRequested;

    /// <summary>
    /// Initializes a new instance of the <see cref="DebounceTimerBase"/> class.
    /// </summary>
    /// <param name="period">The time interval to wait before executing the handler.</param>
    /// <param name="logger">The logger instance for tracing operations.</param>
    protected DebounceTimerBase(int period, ILogger logger)
        : base(logger)
    {
        _period = period;
    }

    /// <summary>
    /// Changes the time interval to wait before executing the handler.
    /// </summary>
    /// <param name="period">The new time interval in milliseconds.</param>
    public void Change(int period)
    {
        _period = period;
    }

    /// <summary>
    /// Requests the timer to execute the handler after the specified period.
    /// </summary>
    public void Request()
    {
        // Volatile read so cross-thread observers see Dispose's Interlocked.Exchange(_disposed, 1) in
        // a timely fashion on weakly-ordered architectures (ARM64). The race between this check and
        // Change below is still possible, but the catch (ObjectDisposedException) below handles it.
        if (IsDisposed)
            return;

        // Set the requested flag BEFORE arming the timer so that if the callback fires between these two
        // statements, its finally-block CompareExchange observes _isRequested == 1 and re-fires the timer.
        // Otherwise the request would be silently lost when the timer fires before the Exchange completes.
        Interlocked.Exchange(ref _isRequested, 1);
        try
        {
            Change(_period, Timeout.Infinite);
        }
        catch (ObjectDisposedException)
        {
            // Race: Dispose() ran between the IsDisposed check above and Change. The intent of this
            // call was to schedule a future firing, which Dispose() has already prevented; swallow safely.
            // This guard MUST be here even though Request() also checks IsDisposed at entry, because the
            // check and Change are not atomic. The same race fires from Callback's finally re-call.
        }
    }

    /// <summary>
    /// Claims the pending request flag inside the gate's critical section so that a fresh request arriving
    /// after the callback starts will trigger the post-release re-fire in <see cref="OnAfterGateReleased"/>.
    /// </summary>
    protected override void OnAfterGateAcquired()
    {
        Interlocked.Exchange(ref _isRequested, 0);
    }

    /// <summary>
    /// Re-fires the timer if a fresh request arrived during <c>HandleAsync</c>. Skipped during dispose;
    /// <see cref="Request"/> also guards on <see cref="TimerBase.IsDisposed"/>.
    /// </summary>
    protected override void OnAfterGateReleased()
    {
        if (!IsDisposed && Interlocked.CompareExchange(ref _isRequested, 0, 1) == 1)
            Request();
    }
}
