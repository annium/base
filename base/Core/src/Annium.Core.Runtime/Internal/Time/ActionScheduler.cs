using System;
using System.Threading;
using System.Threading.Tasks;
using NodaTime;

namespace Annium.Core.Runtime.Internal.Time;

/// <summary>
/// Internal implementation of action scheduler for real-time scheduling
/// </summary>
internal class ActionScheduler : IActionScheduler
{
    /// <summary>
    /// Schedules an action to be executed after a specified timeout in milliseconds
    /// </summary>
    /// <param name="handle">The action to execute</param>
    /// <param name="timeout">The timeout in milliseconds</param>
    /// <returns>A cancellation action</returns>
    public Action Delay(Action handle, int timeout) => Delay(handle, Duration.FromMilliseconds(timeout));

    /// <summary>
    /// Schedules an action to be executed after a specified timeout duration
    /// </summary>
    /// <param name="handle">The action to execute</param>
    /// <param name="timeout">The timeout duration</param>
    /// <returns>A cancellation action</returns>
    public Action Delay(Action handle, Duration timeout)
    {
        var execute = true;
        Task.Delay(timeout.ToTimeSpan())
            .ContinueWith(_ =>
            {
                if (execute)
                    handle();
            })
            .GetAwaiter();

        return () => execute = false;
    }

    /// <summary>
    /// Schedules an action to be executed repeatedly at specified intervals in milliseconds
    /// </summary>
    /// <param name="handle">The action to execute</param>
    /// <param name="interval">The interval in milliseconds</param>
    /// <returns>A cancellation action</returns>
    public Action Interval(Action handle, int interval) => Interval(handle, Duration.FromMilliseconds(interval));

    /// <summary>
    /// Schedules an action to be executed repeatedly at specified interval duration
    /// </summary>
    /// <param name="handle">The action to execute</param>
    /// <param name="interval">The interval duration</param>
    /// <returns>A cancellation action</returns>
    public Action Interval(Action handle, Duration interval)
    {
        var span = interval.ToTimeSpan();
        var timer = new Timer(_ => handle(), null, span, span);

        return () => timer.Dispose();
    }
}
