using System;

namespace Annium.Threading;

/// <summary>
/// Represents a timer that executes operations sequentially.
/// </summary>
public interface ISequentialTimer : IDisposable
{
    /// <summary>
    /// Changes the start time and interval between method invocations for a timer.
    /// </summary>
    /// <param name="dueTime">The amount of time to delay before the invoking the callback method, in milliseconds.</param>
    /// <param name="period">The time interval between invocations of the callback method, in milliseconds.</param>
    /// <returns>true if the timer was successfully updated; otherwise, false.</returns>
    bool Change(int dueTime, int period);

    /// <summary>
    /// Changes the start time and the interval between method invocations for a timer.
    /// </summary>
    /// <param name="dueTime">The amount of time to delay before the first execution.</param>
    /// <param name="period">The time interval between executions.</param>
    /// <returns>true if the timer was successfully updated; otherwise, false.</returns>
    bool Change(TimeSpan dueTime, TimeSpan period);
}
