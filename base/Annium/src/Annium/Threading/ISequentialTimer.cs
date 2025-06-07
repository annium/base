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
    void Change(int dueTime, int period);
}
