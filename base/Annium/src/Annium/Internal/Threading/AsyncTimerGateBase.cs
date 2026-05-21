using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Logging;

namespace Annium.Internal.Threading;

/// <summary>
/// Common scaffolding for asynchronous timer bases (<see cref="AsyncTimerBase"/>,
/// <see cref="DebounceTimerBase"/>): owns the in-flight gate, the re-entrant-dispose flag, the gate-drain on
/// dispose, and the async-void callback dispatcher. Per-subclass hooks (claiming a pending request,
/// re-firing on a deferred trigger) are surfaced via <see cref="OnAfterGateAcquired"/> and
/// <see cref="OnAfterGateReleased"/>.
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
internal abstract class AsyncTimerGateBase : TimerBase
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
    /// Initializes a new instance of the <see cref="AsyncTimerGateBase"/> class with an inert timer; derived ctors
    /// MUST call <see cref="TimerBase.Start"/> as their last step to begin firing (or, for debounce-style timers,
    /// simply not start).
    /// </summary>
    /// <param name="logger">The logger instance for tracing operations.</param>
    protected AsyncTimerGateBase(ILogger logger)
        : base(logger) { }

    /// <summary>
    /// Executes the timer's asynchronous handler.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected abstract ValueTask HandleAsync();

    /// <summary>
    /// Hook called after the in-flight gate has been acquired and before <c>_inCallback</c> is set. Used by
    /// debounce-style subclasses to claim a pending request flag inside the gate's critical section. Default: no-op.
    /// </summary>
    protected virtual void OnAfterGateAcquired() { }

    /// <summary>
    /// Hook called after the in-flight gate has been released. Used by debounce-style subclasses to re-fire
    /// the timer if a fresh request arrived during <c>HandleAsync</c>. Default: no-op.
    /// </summary>
    protected virtual void OnAfterGateReleased() { }

    /// <summary>
    /// Returns whether the current Dispose call is re-entrant (i.e. invoked from inside the callback itself).
    /// Re-entrant disposal skips the drain to avoid self-deadlock — the in-flight callback owns the gate.
    /// </summary>
    /// <returns><see langword="true"/> when Dispose was called from within the callback; otherwise <see langword="false"/>.</returns>
    protected sealed override bool IsReentrantDispose() => _inCallback.Value;

    /// <summary>
    /// Drains any in-flight async callback by acquiring the gate with a bounded wait, then disposes the gate.
    /// On wait timeout the gate is intentionally leaked to avoid <see cref="ObjectDisposedException"/> on the
    /// ThreadPool thread that may still be racing toward the callback's <c>Release()</c>.
    /// </summary>
    protected sealed override void OnDrainCompleted()
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
    /// and traps exceptions so the timer keeps firing on subsequent ticks. Subclasses customise the
    /// callback flow via <see cref="OnAfterGateAcquired"/> and <see cref="OnAfterGateReleased"/>.
    /// </summary>
    /// <param name="state">The timer state object (unused).</param>
    // VSTHRD100: timer callback must be void; exceptions are caught inside the method body.
#pragma warning disable VSTHRD100
    protected sealed override async void InvokeCallback(object? state)
#pragma warning restore VSTHRD100
    {
        // Non-blocking gate acquisition: if Dispose is draining (or another callback is running), skip.
        // The gate (SemaphoreSlim(1,1)) provides exclusive mutex.
        if (!_gate.Wait(0))
            return;

        OnAfterGateAcquired();

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
            // Release the gate BEFORE the post-callback hook so that (a) the freshly armed timer's callback
            // can acquire it cleanly even when the period is sub-millisecond, and (b) _gate.Dispose() in
            // OnDrainCompleted can succeed since its Wait happens-after this Release.
            _gate.Release();
            OnAfterGateReleased();
        }
    }
}
