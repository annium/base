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
    /// Initializes a new instance of the <see cref="AsyncTimerBase"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for tracing operations.</param>
    protected AsyncTimerBase(ILogger logger)
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
        if (Interlocked.CompareExchange(ref _isHandling, 1, 0) == 1)
        {
            return;
        }

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
            _isHandling = 0;
        }
    }
}
