using System;
using Annium.Core.Runtime.Time;
using NodaTime;

namespace Annium.Core.Runtime.Internal.Time;

/// <summary>
/// Internal implementation of action scheduler for managed time scheduling
/// </summary>
internal class ManagedActionScheduler : IActionScheduler
{
    /// <summary>
    /// The time manager used for time tracking
    /// </summary>
    private readonly ITimeManager _timeManager;

    /// <summary>
    /// Initializes a new instance of ManagedActionScheduler with the specified time manager
    /// </summary>
    /// <param name="timeManager">The time manager to use for scheduling</param>
    public ManagedActionScheduler(ITimeManager timeManager)
    {
        _timeManager = timeManager;
    }

    /// <summary>
    /// Schedules an action to be executed after a specified timeout in milliseconds using managed time
    /// </summary>
    /// <param name="handle">The action to execute</param>
    /// <param name="timeout">The timeout in milliseconds</param>
    /// <returns>A cancellation action</returns>
    public Action Delay(Action handle, int timeout) => Delay(handle, Duration.FromMilliseconds(timeout));

    /// <summary>
    /// Schedules an action to be executed after a specified timeout duration using managed time
    /// </summary>
    /// <param name="handle">The action to execute</param>
    /// <param name="timeout">The timeout duration</param>
    /// <returns>A cancellation action</returns>
    public Action Delay(Action handle, Duration timeout)
    {
        var lasting = Duration.Zero;

        _timeManager.OnNowChanged += CheckTime;

        void CheckTime(Duration duration)
        {
            lasting += duration;
            if (lasting < timeout)
                return;
            _timeManager.OnNowChanged -= CheckTime;
            handle();
        }

        return () => _timeManager.OnNowChanged -= CheckTime;
    }

    /// <summary>
    /// Schedules an action to be executed repeatedly at specified intervals in milliseconds using managed time
    /// </summary>
    /// <param name="handle">The action to execute</param>
    /// <param name="interval">The interval in milliseconds</param>
    /// <returns>A cancellation action</returns>
    public Action Interval(Action handle, int interval) => Interval(handle, Duration.FromMilliseconds(interval));

    /// <summary>
    /// Schedules an action to be executed repeatedly at specified interval duration using managed time
    /// </summary>
    /// <param name="handle">The action to execute</param>
    /// <param name="interval">The interval duration</param>
    /// <returns>A cancellation action</returns>
    public Action Interval(Action handle, Duration interval)
    {
        var lasting = Duration.Zero;

        _timeManager.OnNowChanged += CheckTime;

        void CheckTime(Duration duration)
        {
            lasting += duration;
            if (lasting < interval)
                return;

            lasting -= interval;
            handle();
        }

        return () => _timeManager.OnNowChanged -= CheckTime;
    }
}
