using System;

namespace Annium.Threading;

/// <summary>
/// Represents a timer that executes operations sequentially.
/// </summary>
/// <remarks>
/// Implements both <see cref="IDisposable"/> and <see cref="IAsyncDisposable"/> so the drain contract
/// (the bounded wait for an in-flight callback to complete during disposal) is visible at the interface
/// level. Callers may use either <c>using</c> or <c>await using</c>.
/// </remarks>
public interface ISequentialTimer : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Changes the start time and interval between method invocations for a timer.
    /// </summary>
    /// <param name="dueTime">The amount of time to delay before invoking the callback method, in milliseconds.</param>
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
