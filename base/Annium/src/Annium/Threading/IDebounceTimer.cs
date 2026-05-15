using System;

namespace Annium.Threading;

/// <summary>
/// Represents a timer that debounces requests by waiting for a specified period before executing.
/// </summary>
/// <remarks>
/// Implements both <see cref="IDisposable"/> and <see cref="IAsyncDisposable"/> so the drain contract
/// (the bounded wait for an in-flight async callback to complete during disposal) is visible at the
/// interface level. Callers may use either <c>using</c> or <c>await using</c>.
/// </remarks>
public interface IDebounceTimer : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Changes the period of the debounce timer.
    /// </summary>
    /// <param name="period">The new period in milliseconds.</param>
    void Change(int period);

    /// <summary>
    /// Changes the period of the debounce timer.
    /// </summary>
    /// <param name="period">The new period.</param>
    void Change(TimeSpan period);

    /// <summary>
    /// Requests the debounce timer to execute after the current period.
    /// </summary>
    void Request();
}
