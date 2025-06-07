using System;
using NodaTime;

namespace Annium;

/// <summary>
/// Provides functionality for scheduling actions to be executed after a delay or at regular intervals.
/// </summary>
public interface IActionScheduler
{
    /// <summary>
    /// Schedules an action to be executed after the specified timeout.
    /// </summary>
    /// <param name="handle">The action to execute when the timeout expires.</param>
    /// <param name="timeout">The timeout in milliseconds before executing the action.</param>
    /// <returns>An action that can be used to cancel the scheduled execution.</returns>
    Action Delay(Action handle, int timeout);

    /// <summary>
    /// Schedules an action to be executed after the specified timeout.
    /// </summary>
    /// <param name="handle">The action to execute when the timeout expires.</param>
    /// <param name="timeout">The timeout duration before executing the action.</param>
    /// <returns>An action that can be used to cancel the scheduled execution.</returns>
    Action Delay(Action handle, Duration timeout);

    /// <summary>
    /// Schedules an action to be executed repeatedly at the specified interval.
    /// </summary>
    /// <param name="handle">The action to execute at each interval.</param>
    /// <param name="interval">The interval in milliseconds between executions.</param>
    /// <returns>An action that can be used to cancel the scheduled execution.</returns>
    Action Interval(Action handle, int interval);

    /// <summary>
    /// Schedules an action to be executed repeatedly at the specified interval.
    /// </summary>
    /// <param name="handle">The action to execute at each interval.</param>
    /// <param name="interval">The interval duration between executions.</param>
    /// <returns>An action that can be used to cancel the scheduled execution.</returns>
    Action Interval(Action handle, Duration interval);
}
