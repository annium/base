using System;
using System.Threading.Tasks;
using Annium.Internal.Threading;
using Annium.Logging;

namespace Annium.Threading;

/// <summary>
/// Provides factory methods for creating sequential and debounce timers.
/// </summary>
public static class Timers
{
    /// <summary>
    /// Creates a synchronous sequential timer.
    /// </summary>
    /// <param name="handler">The action to execute.</param>
    /// <param name="dueTime">The amount of time to delay before invoking the callback method, in milliseconds.</param>
    /// <param name="period">The time interval between invocations of the callback method, in milliseconds.</param>
    /// <param name="logger">The logger to use for logging.</param>
    /// <returns>An <see cref="ISequentialTimer"/> instance.</returns>
    public static ISequentialTimer Sync(Action handler, int dueTime, int period, ILogger logger)
    {
        return new SyncTimer(handler, dueTime, period, logger);
    }

    /// <summary>
    /// Creates a synchronous sequential timer with state.
    /// </summary>
    /// <typeparam name="T">The type of the state object.</typeparam>
    /// <param name="state">The state object.</param>
    /// <param name="handler">The action to execute with the state.</param>
    /// <param name="dueTime">The amount of time to delay before invoking the callback method, in milliseconds.</param>
    /// <param name="period">The time interval between invocations of the callback method, in milliseconds.</param>
    /// <param name="logger">The logger to use for logging.</param>
    /// <returns>An <see cref="ISequentialTimer"/> instance.</returns>
    public static ISequentialTimer Sync<T>(T state, Action<T> handler, int dueTime, int period, ILogger logger)
        where T : class
    {
        return new SyncTimer<T>(state, handler, dueTime, period, logger);
    }

#pragma warning disable VSTHRD200
    /// <summary>
    /// Creates an asynchronous sequential timer.
    /// </summary>
    /// <param name="handler">The asynchronous function to execute.</param>
    /// <param name="dueTime">The amount of time to delay before invoking the callback method, in milliseconds.</param>
    /// <param name="period">The time interval between invocations of the callback method, in milliseconds.</param>
    /// <param name="logger">The logger to use for logging.</param>
    /// <returns>An <see cref="ISequentialTimer"/> instance.</returns>
    public static ISequentialTimer Async(Func<ValueTask> handler, int dueTime, int period, ILogger logger)
#pragma warning restore VSTHRD200
    {
        return new AsyncTimer(handler, dueTime, period, logger);
    }

#pragma warning disable VSTHRD200
    /// <summary>
    /// Creates an asynchronous sequential timer with state.
    /// </summary>
    /// <typeparam name="T">The type of the state object.</typeparam>
    /// <param name="state">The state object.</param>
    /// <param name="handler">The asynchronous function to execute with the state.</param>
    /// <param name="dueTime">The amount of time to delay before invoking the callback method, in milliseconds.</param>
    /// <param name="period">The time interval between invocations of the callback method, in milliseconds.</param>
    /// <param name="logger">The logger to use for logging.</param>
    /// <returns>An <see cref="ISequentialTimer"/> instance.</returns>
    public static ISequentialTimer Async<T>(
#pragma warning restore VSTHRD200
        T state,
        Func<T, ValueTask> handler,
        int dueTime,
        int period,
        ILogger logger
    )
        where T : class
    {
        return new AsyncTimer<T>(state, handler, dueTime, period, logger);
    }

    /// <summary>
    /// Creates a debounce timer.
    /// </summary>
    /// <param name="handler">The asynchronous function to execute.</param>
    /// <param name="period">The debounce period in milliseconds.</param>
    /// <param name="logger">The logger to use for logging.</param>
    /// <returns>An <see cref="IDebounceTimer"/> instance.</returns>
    public static IDebounceTimer Debounce(Func<ValueTask> handler, int period, ILogger logger)
    {
        return new DebounceTimer(handler, period, logger);
    }

    /// <summary>
    /// Creates a debounce timer with state.
    /// </summary>
    /// <typeparam name="T">The type of the state object.</typeparam>
    /// <param name="state">The state object.</param>
    /// <param name="handler">The asynchronous function to execute with the state.</param>
    /// <param name="period">The debounce period in milliseconds.</param>
    /// <param name="logger">The logger to use for logging.</param>
    /// <returns>An <see cref="IDebounceTimer"/> instance.</returns>
    public static IDebounceTimer Debounce<T>(T state, Func<T, ValueTask> handler, int period, ILogger logger)
        where T : class
    {
        return new DebounceTimer<T>(state, handler, period, logger);
    }
}
