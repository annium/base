using System;

namespace Annium.Threading;

/// <summary>
/// Represents a timer that debounces requests by waiting for a specified period before executing.
/// </summary>
public interface IDebounceTimer : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Changes the period of the debounce timer.
    /// </summary>
    /// <param name="period">The new period in milliseconds.</param>
    void Change(int period);

    /// <summary>
    /// Requests the debounce timer to execute after the current period.
    /// </summary>
    void Request();
}
