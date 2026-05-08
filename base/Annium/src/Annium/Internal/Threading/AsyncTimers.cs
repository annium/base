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
internal class AsyncTimer<T> : AsyncTimerBase
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
        Timer = new Timer(Callback, null, dueTime, period);
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
internal class AsyncTimer : AsyncTimerBase
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
        Timer = new Timer(Callback, null, dueTime, period);
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
internal abstract class AsyncTimerBase : ISequentialTimer, ILogSubject
{
    /// <summary>
    /// Maximum time <see cref="Dispose"/> waits for an in-flight callback before returning.
    /// </summary>
    private static readonly TimeSpan _disposeWaitBudget = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Gets the logger instance for tracing operations.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Gets the underlying timer instance.
    /// </summary>
    protected Timer Timer { get; init; } = default!;

    /// <summary>
    /// Mutex + in-flight signal. The single permit is held for the duration of an executing callback;
    /// <see cref="Callback"/> uses non-blocking acquisition (skips overlapping ticks), and <see cref="Dispose"/>
    /// uses a bounded blocking acquisition to drain any in-flight callback before reclaiming owned state.
    /// </summary>
    private readonly SemaphoreSlim _gate = new(initialCount: 1, maxCount: 1);

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncTimerBase"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for tracing operations.</param>
    protected AsyncTimerBase(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Releases all resources used by the timer. After disposing the underlying timer (stopping new firings),
    /// drains any in-flight callback by acquiring the gate's permit (bounded by <see cref="_disposeWaitBudget"/>).
    /// On success, the gate is disposed; on timeout, the gate is intentionally leaked so the still-running callback
    /// can release without raising <see cref="ObjectDisposedException"/>.
    /// </summary>
    public void Dispose()
    {
        Timer.Dispose();
        if (_gate.Wait(_disposeWaitBudget))
            _gate.Dispose();
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
    protected async void Callback(object? _)
#pragma warning restore VSTHRD100
    {
        // Non-blocking acquisition: if another callback is in flight (or Dispose is draining), skip this tick.
        if (!_gate.Wait(0))
            return;

        try
        {
            await HandleAsync();
        }
        catch (Exception e)
        {
            this.Error(e);
        }
        finally
        {
            // Race with Dispose: if Dispose acquired and disposed the gate while we held the permit,
            // it cannot — Dispose only proceeds once it owns the permit. Release here is always safe.
            _gate.Release();
        }
    }
}
