using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;
using Annium.Threading;

namespace Annium.Internal.Threading;

/// <summary>
/// Provides an asynchronous timer that executes a handler with a state object at specified intervals.
/// </summary>
/// <typeparam name="T">The type of the state object.</typeparam>
internal sealed class AsyncTimer<T> : AsyncTimerBase
    where T : class
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
    /// Initializes a new instance of the <see cref="AsyncTimer{T}"/> class.
    /// </summary>
    /// <param name="state">The state object to pass to the handler.</param>
    /// <param name="handler">The asynchronous handler to execute.</param>
    /// <param name="dueTime">The amount of time to delay before the first execution.</param>
    /// <param name="period">The time interval between executions.</param>
    /// <param name="logger">The logger instance for tracing operations.</param>
    public AsyncTimer(T state, Func<T, ValueTask> handler, int dueTime, int period, ILogger logger)
        : base(logger)
    {
        _state = state;
        _handler = handler;
        Start(dueTime, period);
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
/// Provides an asynchronous timer that executes a handler at specified intervals.
/// </summary>
internal sealed class AsyncTimer : AsyncTimerBase
{
    /// <summary>
    /// The asynchronous handler to execute.
    /// </summary>
    private readonly Func<ValueTask> _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncTimer"/> class.
    /// </summary>
    /// <param name="handler">The asynchronous handler to execute.</param>
    /// <param name="dueTime">The amount of time to delay before the first execution.</param>
    /// <param name="period">The time interval between executions.</param>
    /// <param name="logger">The logger instance for tracing operations.</param>
    public AsyncTimer(Func<ValueTask> handler, int dueTime, int period, ILogger logger)
        : base(logger)
    {
        _handler = handler;
        Start(dueTime, period);
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
/// Provides a base class for asynchronous timers.
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
/// callers MUST NOT free shared state on timeout without independent synchronization.
/// </remarks>
internal abstract class AsyncTimerBase : TimerBase, ISequentialTimer
{
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
    /// Initializes a new instance of the <see cref="AsyncTimerBase"/> class with an inert timer; derived ctors
    /// MUST call <see cref="TimerBase.Start"/> as their last step to begin firing.
    /// </summary>
    /// <param name="logger">The logger instance for tracing operations.</param>
    protected AsyncTimerBase(ILogger logger)
        : base(logger) { }

    /// <summary>
    /// Changes the start time and the interval between method invocations for a timer.
    /// </summary>
    /// <param name="dueTime">The amount of time to delay before the first execution.</param>
    /// <param name="period">The time interval between executions.</param>
    /// <returns>true if the timer was successfully updated; otherwise, false.</returns>
    public bool Change(int dueTime, int period)
    {
        return ChangeTimer(dueTime, period);
    }

    /// <summary>
    /// Changes the start time and the interval between method invocations for a timer.
    /// </summary>
    /// <param name="dueTime">The amount of time to delay before the first execution.</param>
    /// <param name="period">The time interval between executions.</param>
    /// <returns>true if the timer was successfully updated; otherwise, false.</returns>
    public bool Change(TimeSpan dueTime, TimeSpan period)
    {
        return ChangeTimer(dueTime, period);
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
        // Non-blocking acquisition: if another callback is in flight (or Dispose is draining), skip this tick.
        if (!_gate.Wait(0))
            return;

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
            // Release is always safe against the gate's own teardown: _gate.Dispose() in OnDrainCompleted
            // only runs AFTER _gate.Wait() returns, which in turn only returns after this very Release —
            // so the release happens-before the dispose. The two-phase drain (WaitHandle then _gate.Wait)
            // is required precisely BECAUSE this callback can still be racing toward Release after the
            // WaitHandle drain completes (async void returns at the first await).
            _gate.Release();
        }
    }
}
