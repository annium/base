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
/// On <see cref="Dispose"/>, the underlying <see cref="Timer"/> is drained via the
/// <see cref="Timer.Dispose(WaitHandle)"/> overload before the in-flight gate is reclaimed; this drains queued
/// ThreadPool callbacks before the gate is torn down, eliminating <see cref="ObjectDisposedException"/>
/// from the gate's <see cref="SemaphoreSlim.Wait(int)"/> in <see cref="Callback"/>. If the drain or the
/// gate acquisition exceeds <see cref="_disposeWaitBudget"/>, both wait handle and gate are intentionally
/// leaked so the still-running callback can complete safely; a warning is logged in that case.
/// Callers MUST NOT free shared state on timeout without independent synchronization.
/// </remarks>
internal abstract class AsyncTimerBase : ISequentialTimer, ILogSubject
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
    /// The underlying timer instance. Created in the base ctor with <see cref="Timeout.Infinite"/> and started by
    /// the derived ctor's call to <see cref="Start"/> after derived fields are assigned.
    /// </summary>
    protected Timer Timer { get; }

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
    /// 0 if <see cref="Dispose"/> has not yet claimed the dispose path; 1 once it has. Prevents a concurrent
    /// second caller from racing into <c>_gate.Wait</c> on the already-disposed semaphore.
    /// </summary>
    private int _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncTimerBase"/> class with an inert timer; derived ctors
    /// MUST call <see cref="Start"/> as their last step to begin firing.
    /// </summary>
    /// <param name="logger">The logger instance for tracing operations.</param>
    protected AsyncTimerBase(ILogger logger)
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
    /// <para>
    /// The <see cref="Timer.Dispose(WaitHandle)"/> overload only stops new callbacks from being scheduled;
    /// because <see cref="Callback"/> is <c>async void</c>, the wait handle is signaled the moment the
    /// synchronous prefix of <c>Callback</c> returns (typically at <c>await HandleAsync()</c>) — not when
    /// the asynchronous handler completes. The actual in-flight drain is provided by acquiring
    /// <see cref="_gate"/>'s permit, which a running callback releases only in its <c>finally</c> block.
    /// On either timeout, the wait handle and (on the second) the gate are intentionally leaked and a
    /// warning is logged so the still-running callback can complete without raising
    /// <see cref="ObjectDisposedException"/>; callers MUST NOT free shared state on timeout without
    /// independent synchronization.
    /// </para>
    /// <para>
    /// Re-entrant disposal — calling <see cref="Dispose"/> from inside <c>HandleAsync</c> (or any
    /// continuation of it) — is detected via <see cref="_inCallback"/> and skips the gate drain to
    /// avoid the self-deadlock that would otherwise occur. In that mode the timer is stopped but the
    /// gate is intentionally not disposed; the handler's <c>finally</c> still releases the permit.
    /// </para>
    /// </remarks>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
            return;

        if (_inCallback.Value)
        {
            // Re-entrant dispose from inside HandleAsync's logical flow: the calling flow holds the gate
            // permit, so blocking on _gate.Wait() would deadlock. Stop the timer and return; the handler's
            // finally will release the permit on its own. The gate is intentionally not disposed here.
            Timer.Dispose();
            return;
        }

        var drained = new ManualResetEvent(false);
        Timer.Dispose(drained);

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
            // Release is always safe: the timer drain in Dispose runs BEFORE _gate.Wait in Dispose, so by the
            // time Dispose acquires the gate, no queued callback can still be racing toward this Release.
            _gate.Release();
        }
    }
}
