using System;

namespace Annium.Extensions.Jobs;

/// <summary>
/// Provides utility methods for creating and starting timers
/// </summary>
public static class Timer
{
    /// <summary>
    /// Starts a timer with integer timing values
    /// </summary>
    /// <param name="handle">The action to execute</param>
    /// <param name="dueTime">The time before first execution</param>
    /// <param name="period">The period between executions</param>
    /// <returns>A disposable timer</returns>
    public static IDisposable Start(Action handle, int dueTime, int period) =>
        new System.Threading.Timer(_ => handle(), null, dueTime, period);

    /// <summary>
    /// Starts a timer with unsigned integer timing values
    /// </summary>
    /// <param name="handle">The action to execute</param>
    /// <param name="dueTime">The time before first execution</param>
    /// <param name="period">The period between executions</param>
    /// <returns>A disposable timer</returns>
    public static IDisposable Start(Action handle, uint dueTime, uint period) =>
        new System.Threading.Timer(_ => handle(), null, dueTime, period);

    /// <summary>
    /// Starts a timer with long timing values
    /// </summary>
    /// <param name="handle">The action to execute</param>
    /// <param name="dueTime">The time before first execution</param>
    /// <param name="period">The period between executions</param>
    /// <returns>A disposable timer</returns>
    public static IDisposable Start(Action handle, long dueTime, long period) =>
        new System.Threading.Timer(_ => handle(), null, dueTime, period);

    /// <summary>
    /// Starts a timer with TimeSpan timing values
    /// </summary>
    /// <param name="handle">The action to execute</param>
    /// <param name="dueTime">The time before first execution</param>
    /// <param name="period">The period between executions</param>
    /// <returns>A disposable timer</returns>
    public static IDisposable Start(Action handle, TimeSpan dueTime, TimeSpan period) =>
        new System.Threading.Timer(_ => handle(), null, dueTime, period);

    /// <summary>
    /// Starts a timer with state and integer timing values
    /// </summary>
    /// <typeparam name="T">The type of state</typeparam>
    /// <param name="handle">The action to execute with state</param>
    /// <param name="state">The state to pass to the action</param>
    /// <param name="dueTime">The time before first execution</param>
    /// <param name="period">The period between executions</param>
    /// <returns>A disposable timer</returns>
    public static IDisposable Start<T>(Action<T> handle, T state, int dueTime, int period) =>
        new System.Threading.Timer(x => handle((T)x!), state, dueTime, period);

    /// <summary>
    /// Starts a timer with state and unsigned integer timing values
    /// </summary>
    /// <typeparam name="T">The type of state</typeparam>
    /// <param name="handle">The action to execute with state</param>
    /// <param name="state">The state to pass to the action</param>
    /// <param name="dueTime">The time before first execution</param>
    /// <param name="period">The period between executions</param>
    /// <returns>A disposable timer</returns>
    public static IDisposable Start<T>(Action<T> handle, T state, uint dueTime, uint period) =>
        new System.Threading.Timer(x => handle((T)x!), state, dueTime, period);

    /// <summary>
    /// Starts a timer with state and long timing values
    /// </summary>
    /// <typeparam name="T">The type of state</typeparam>
    /// <param name="handle">The action to execute with state</param>
    /// <param name="state">The state to pass to the action</param>
    /// <param name="dueTime">The time before first execution</param>
    /// <param name="period">The period between executions</param>
    /// <returns>A disposable timer</returns>
    public static IDisposable Start<T>(Action<T> handle, T state, long dueTime, long period) =>
        new System.Threading.Timer(x => handle((T)x!), state, dueTime, period);

    /// <summary>
    /// Starts a timer with state and TimeSpan timing values
    /// </summary>
    /// <typeparam name="T">The type of state</typeparam>
    /// <param name="handle">The action to execute with state</param>
    /// <param name="state">The state to pass to the action</param>
    /// <param name="dueTime">The time before first execution</param>
    /// <param name="period">The period between executions</param>
    /// <returns>A disposable timer</returns>
    public static IDisposable Start<T>(Action<T> handle, T state, TimeSpan dueTime, TimeSpan period) =>
        new System.Threading.Timer(x => handle((T)x!), state, dueTime, period);
}
