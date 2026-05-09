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
    /// A flag indicating whether <see cref="Dispose"/> has run. Set BEFORE <c>_timer.Dispose()</c>
    /// so concurrent <see cref="Request"/> / <see cref="Callback"/> observe the dispose and skip the
    /// <see cref="Timer.Change(int, int)"/> call that would otherwise throw <see cref="ObjectDisposedException"/>
    /// silently into the threadpool.
    /// </summary>
    private volatile bool _disposed;

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
        _disposed = true;
        _timer.Dispose();
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
        if (_disposed)
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
        if (Interlocked.CompareExchange(ref _isHandling, 1, 0) == 1)
            return;

        // Claim the pending request that triggered this callback.
        Interlocked.Exchange(ref _isRequested, 0);

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
            Interlocked.Exchange(ref _isHandling, 0);

            // Atomically consume any request that arrived during HandleAsync; if claimed, re-fire.
            // Skip if Dispose ran during the handler — Request() also guards on _disposed.
            if (!_disposed && Interlocked.CompareExchange(ref _isRequested, 0, 1) == 1)
                Request();
        }
    }
}
