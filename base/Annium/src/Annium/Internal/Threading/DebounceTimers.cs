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
/// On <see cref="Dispose"/>, the underlying <see cref="Timer"/> is drained via the
/// <see cref="Timer.Dispose(WaitHandle)"/> overload before the in-flight gate is reclaimed; this drains
/// queued ThreadPool callbacks before the gate is torn down. Because <see cref="Callback"/> is
/// <c>async void</c>, the wait handle is signaled the moment the synchronous prefix of <c>Callback</c>
/// returns (typically at <c>await HandleAsync()</c>) — not when the asynchronous handler completes.
/// The actual in-flight drain is provided by acquiring <see cref="_gate"/>'s permit, which a running
/// callback releases only in its <c>finally</c> block. On either timeout, the wait handle and (on the
/// second) the gate are intentionally leaked and a warning is logged so the still-running callback can
/// complete without raising <see cref="ObjectDisposedException"/>; callers MUST NOT free shared state
/// on timeout without independent synchronization. Re-entrant disposal — calling <see cref="Dispose"/>
/// from inside <c>HandleAsync</c> (or any continuation of it) — is detected via <see cref="_inCallback"/>
/// and skips the gate drain to avoid the self-deadlock that would otherwise occur.
/// </remarks>
internal abstract class DebounceTimerBase : IDebounceTimer, ILogSubject
{
    /// <summary>
    /// Maximum time <see cref="Dispose"/> waits for the timer drain and the in-flight callback before returning.
    /// </summary>
    private static readonly TimeSpan _disposeWaitBudget = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Gets the logger instance for tracing operations.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The underlying timer instance.
    /// </summary>
    private readonly Timer _timer;

    /// <summary>
    /// The time interval to wait before executing the handler. Volatile so cross-thread reads in
    /// <see cref="Request"/> observe writes from <see cref="Change(int)"/> without a stale value
    /// on weakly-ordered architectures.
    /// </summary>
    private volatile int _period;

    /// <summary>
    /// A flag indicating whether a new request has been made (1) or not (0).
    /// </summary>
    private volatile int _isRequested;

    /// <summary>
    /// A flag indicating whether the timer is currently handling a callback (1) or not (0).
    /// </summary>
    private volatile int _isHandling;

    /// <summary>
    /// 0 if <see cref="Dispose"/> has not yet claimed the dispose path; 1 once it has. Set BEFORE the
    /// drain begins so concurrent <see cref="Request"/> / <see cref="Callback"/> observe the dispose and
    /// short-circuit. Also prevents a concurrent second <see cref="Dispose"/> caller from racing into
    /// <c>_gate.Wait</c> on the already-disposed semaphore.
    /// </summary>
    private int _disposed;

    /// <summary>
    /// Mutex + in-flight signal. The single permit is held for the duration of an executing callback;
    /// <see cref="Callback"/> uses non-blocking acquisition (skips overlapping ticks), and <see cref="Dispose"/>
    /// uses a bounded blocking acquisition to drain any in-flight callback before reclaiming owned state.
    /// </summary>
    private readonly SemaphoreSlim _gate = new(initialCount: 1, maxCount: 1);

    /// <summary>
    /// Per-instance flow flag set inside <see cref="Callback"/> so a re-entrant <see cref="Dispose"/> call
    /// (one made from the handler's logical execution context, including across <c>await</c>s) can skip the
    /// gate drain — the calling flow holds the permit, so attempting to acquire it would deadlock.
    /// </summary>
    private readonly AsyncLocal<bool> _inCallback = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DebounceTimerBase"/> class.
    /// </summary>
    /// <param name="period">The time interval to wait before executing the handler.</param>
    /// <param name="logger">The logger instance for tracing operations.</param>
    protected DebounceTimerBase(int period, ILogger logger)
    {
        Logger = logger;
        _timer = new Timer(Callback, null, Timeout.Infinite, Timeout.Infinite);
        _period = period;
    }

    /// <summary>
    /// Releases all resources used by the timer. Drains queued ThreadPool callbacks via
    /// <see cref="Timer.Dispose(WaitHandle)"/>, then acquires the gate's permit (bounded by
    /// <see cref="_disposeWaitBudget"/>) so any in-flight callback completes. On success the wait handle
    /// and the gate are both disposed; on timeout, both are leaked and a warning is logged so the
    /// still-running callback can release its state without raising <see cref="ObjectDisposedException"/>.
    /// </summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
            return;

        if (_inCallback.Value)
        {
            // Re-entrant dispose from inside HandleAsync's logical flow: the calling flow holds the gate
            // permit, so blocking on _gate.Wait() would deadlock. Stop the timer and return; the handler's
            // finally will release the permit on its own. The gate is intentionally not disposed here.
            _timer.Dispose();
            return;
        }

        var drained = new ManualResetEvent(false);
        _timer.Dispose(drained);

        if (drained.WaitOne(_disposeWaitBudget))
        {
            drained.Dispose();
            if (_gate.Wait(_disposeWaitBudget))
            {
                _gate.Dispose();
                return;
            }

            this.Warn(
                "Timer disposed but in-flight callback exceeded {budget} drain budget; gate intentionally leaked",
                _disposeWaitBudget
            );
            return;
        }

        // Drain timed out: queued ThreadPool callbacks may still execute. Leak both handles to keep them safe.
        this.Warn(
            "Timer drain exceeded {budget} budget; wait handle and gate intentionally leaked to allow queued callbacks to complete",
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
        // _timer.Change below is still possible, but the catch (ObjectDisposedException) below handles it.
        if (Volatile.Read(ref _disposed) != 0)
            return;

        // Set the requested flag BEFORE arming the timer so that if the callback fires between these two
        // statements, its finally-block CompareExchange observes _isRequested == 1 and re-fires the timer.
        // Otherwise the request would be silently lost when the timer fires before the Exchange completes.
        Interlocked.Exchange(ref _isRequested, 1);
        try
        {
            _timer.Change(_period, Timeout.Infinite);
        }
        catch (ObjectDisposedException)
        {
            // Race: Dispose() ran between the _disposed check above and _timer.Change(). The intent of this
            // call was to schedule a future firing, which Dispose() has already prevented; swallow safely.
            // This guard MUST be here even though Request() also checks _disposed at entry, because the
            // check and Change() are not atomic. The same race fires from Callback's finally re-call.
        }
    }

    /// <summary>
    /// Executes the timer's handler.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected abstract ValueTask HandleAsync();

    /// <summary>
    /// The callback method that is called when the timer elapses.
    /// </summary>
    /// <param name="_">The state object passed to the timer (unused).</param>
#pragma warning disable VSTHRD100
    private async void Callback(object? _)
#pragma warning restore VSTHRD100
    {
        // Non-blocking gate acquisition: if Dispose is draining (or another callback is running), skip.
        if (!_gate.Wait(0))
            return;

        if (Interlocked.CompareExchange(ref _isHandling, 1, 0) == 1)
        {
            // Another callback already claimed _isHandling between our gate-wait and this CAS — extremely
            // unlikely given the gate, but release and bail out to keep both invariants consistent.
            _gate.Release();
            return;
        }

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
            Interlocked.Exchange(ref _isHandling, 0);

            // Atomically consume any request that arrived during HandleAsync; if claimed, re-fire.
            // Skip if Dispose ran during the handler — Request() also guards on _disposed.
            // Volatile.Read provides the acquire fence needed to observe Dispose's Interlocked.Exchange.
            if (Volatile.Read(ref _disposed) == 0 && Interlocked.CompareExchange(ref _isRequested, 0, 1) == 1)
                Request();

            // Release is always safe: Dispose's _gate.Wait runs AFTER the timer drain, so by the time it
            // acquires the gate, no queued callback can still be racing toward this Release.
            _gate.Release();
        }
    }
}
