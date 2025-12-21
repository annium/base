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
internal class DebounceTimer<T> : DebounceTimerBase
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
internal class DebounceTimer : DebounceTimerBase
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
internal abstract class DebounceTimerBase : IDebounceTimer, ILogSubject
{
    /// <summary>
    /// Gets the logger instance for tracing operations.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The underlying timer instance.
    /// </summary>
    private readonly Timer _timer;

    /// <summary>
    /// The time interval to wait before executing the handler.
    /// </summary>
    private int _period;

    /// <summary>
    /// A flag indicating whether a new request has been made (1) or not (0).
    /// </summary>
    private volatile int _isRequested;

    /// <summary>
    /// A flag indicating whether the timer is currently handling a callback (1) or not (0).
    /// </summary>
    private volatile int _isHandling;

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
    /// Releases all resources used by the timer.
    /// </summary>
    public void Dispose()
    {
        _timer.Dispose();
        GC.SuppressFinalize(this);
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
        _timer.Change(_period, Timeout.Infinite);
        _isRequested = 1;
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
        if (Interlocked.CompareExchange(ref _isHandling, 1, 0) == 1)
            return;

        _isRequested = 0;

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

            if (_isRequested == 1)
                Request();
        }
    }
}
