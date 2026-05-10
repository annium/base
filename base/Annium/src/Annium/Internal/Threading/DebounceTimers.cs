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
/// Provides a base class for debounced timers.
/// </summary>
/// <remarks>
/// On <see cref="TimerBase.Dispose"/>, the underlying timer is drained via the
/// <see cref="System.Threading.Timer.Dispose(WaitHandle)"/> overload, then <see cref="OnDrainCompleted"/>
/// reclaims the in-flight gate so any still-running async continuation can finish before the gate is
/// torn down. Because <see cref="InvokeCallback"/> is <c>async void</c>, the wait handle is signaled
/// the moment the synchronous prefix returns (typically at <c>await HandleAsync()</c>) — not when the
/// asynchronous handler completes — which is why the gate drain is mandatory after the wait handle
/// drain. On either timeout the wait handle / gate are intentionally leaked and a warning is logged
/// so the still-running callback can complete without raising <see cref="ObjectDisposedException"/>;
/// callers MUST NOT free shared state on timeout without independent synchronization. Re-entrant disposal
/// — calling <see cref="TimerBase.Dispose"/> from inside <c>HandleAsync</c> (or any continuation of it) —
/// is detected via <see cref="_inCallback"/> and skips the gate drain to avoid the self-deadlock that
/// would otherwise occur.
/// </remarks>
internal abstract class DebounceTimerBase : TimerBase, IDebounceTimer
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
    /// Mutex + in-flight signal. The single permit is held for the duration of an executing callback;
    /// <see cref="InvokeCallback"/> uses non-blocking acquisition (skips overlapping ticks), and
    /// <see cref="OnDrainCompleted"/> uses a bounded blocking acquisition to drain any in-flight callback
    /// before reclaiming owned state.
    /// </summary>
    private readonly SemaphoreSlim _gate = new(initialCount: 1, maxCount: 1);

    /// <summary>
    /// Per-instance flow flag set inside <see cref="InvokeCallback"/> so a re-entrant
    /// <see cref="TimerBase.Dispose"/> call (one made from the handler's logical execution context, including
    /// across <c>await</c>s) can skip the gate drain — the calling flow holds the permit, so attempting to
    /// acquire it would deadlock.
    /// </summary>
    private readonly AsyncLocal<bool> _inCallback = new();

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
        // ChangeTimer below is still possible, but the catch (ObjectDisposedException) below handles it.
        if (IsDisposed)
            return;

        // Set the requested flag BEFORE arming the timer so that if the callback fires between these two
        // statements, its finally-block CompareExchange observes _isRequested == 1 and re-fires the timer.
        // Otherwise the request would be silently lost when the timer fires before the Exchange completes.
        Interlocked.Exchange(ref _isRequested, 1);
        try
        {
            ChangeTimer(_period, Timeout.Infinite);
        }
        catch (ObjectDisposedException)
        {
            // Race: Dispose() ran between the IsDisposed check above and ChangeTimer. The intent of this
            // call was to schedule a future firing, which Dispose() has already prevented; swallow safely.
            // This guard MUST be here even though Request() also checks IsDisposed at entry, because the
            // check and ChangeTimer are not atomic. The same race fires from Callback's finally re-call.
        }
    }

    /// <summary>
    /// Executes the timer's handler.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected abstract ValueTask HandleAsync();

    /// <inheritdoc />
    protected override bool IsReentrantDispose() => _inCallback.Value;

    /// <inheritdoc />
    protected override void OnDrainCompleted()
    {
        if (_gate.Wait(TimerConstants.DisposeWaitBudget))
        {
            _gate.Dispose();
            return;
        }

        this.Warn(
            "Timer disposed but in-flight callback exceeded {budget} drain budget; gate intentionally leaked",
            TimerConstants.DisposeWaitBudget
        );
    }

    /// <summary>
    /// The callback invoked by the underlying timer. Runs <see cref="HandleAsync"/> under the in-flight gate
    /// and traps exceptions so the timer keeps firing on subsequent ticks.
    /// </summary>
    /// <param name="state">The timer state object (unused).</param>
#pragma warning disable VSTHRD100
    protected override async void InvokeCallback(object? state)
#pragma warning restore VSTHRD100
    {
        // Non-blocking gate acquisition: if Dispose is draining (or another callback is running), skip.
        // The gate (SemaphoreSlim(1,1)) provides exclusive mutex; no separate _isHandling CAS is needed.
        if (!_gate.Wait(0))
            return;

        // Claim the pending request that triggered this callback.
        Interlocked.Exchange(ref _isRequested, 0);

        var prevInCallback = _inCallback.Value;
        _inCallback.Value = true;
        try
        {
            await HandleAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            this.Error(e);
        }
        finally
        {
            _inCallback.Value = prevInCallback;
            // Release the gate BEFORE re-firing so that the freshly armed timer's callback can acquire it
            // cleanly even when _period is sub-millisecond. If Request() ran first, a fast-firing timer
            // could find the gate still held and silently drop the request.
            _gate.Release();

            // Atomically consume any request that arrived during HandleAsync; if claimed, re-fire.
            // Skip if Dispose ran during the handler — Request() also guards on IsDisposed.
            if (!IsDisposed && Interlocked.CompareExchange(ref _isRequested, 0, 1) == 1)
                Request();
        }
    }
}
