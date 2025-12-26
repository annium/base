using System;
using System.Threading;
using Annium.Logging;
using Annium.Threading;

namespace Annium.Internal.Threading;

/// <summary>
/// Provides a synchronous timer that executes a handler with a state object at specified intervals.
/// </summary>
/// <typeparam name="T">The type of the state object.</typeparam>
internal class SyncTimer<T> : SyncTimerBase
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
        Timer = new Timer(Callback, null, dueTime, period);
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
internal class SyncTimer : SyncTimerBase
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
        Timer = new Timer(Callback, null, dueTime, period);
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
internal abstract class SyncTimerBase : ISequentialTimer, ILogSubject
{
    /// <summary>
    /// Gets the logger instance for tracing operations.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Gets the underlying timer instance.
    /// </summary>
    protected Timer Timer { get; init; } = default!;

    /// <summary>
    /// A flag indicating whether the timer is currently handling a callback (1) or not (0).
    /// </summary>
    private volatile int _isHandling;

    /// <summary>
    /// Initializes a new instance of the <see cref="SyncTimerBase"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for tracing operations.</param>
    protected SyncTimerBase(ILogger logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Releases all resources used by the timer.
    /// </summary>
    public void Dispose()
    {
        Timer.Dispose();
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
    protected void Callback(object? _)
    {
        if (Interlocked.CompareExchange(ref _isHandling, 1, 0) == 1)
        {
            return;
        }

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
            _isHandling = 0;
        }
    }
}
