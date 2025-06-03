using System;
using NodaTime;

namespace Annium.Core.Runtime.Time;

/// <summary>
/// Manages time-related operations and provides time synchronization capabilities.
/// </summary>
/// <remarks>
/// This interface is used to control and monitor time in the application,
/// particularly useful for testing and time-dependent operations.
/// </remarks>
public interface ITimeManager
{
    /// <summary>
    /// Event that is raised when the current time is changed.
    /// </summary>
    /// <remarks>
    /// The event provides the duration of the time change.
    /// Example usage:
    /// <code>
    /// timeManager.OnNowChanged += duration =>
    ///     Console.WriteLine($"Time changed by {duration}");
    /// </code>
    /// </remarks>
    event Action<Duration> OnNowChanged;

    /// <summary>
    /// Gets the current instant in time.
    /// </summary>
    /// <remarks>
    /// Returns the current time as an NodaTime Instant.
    /// Example usage:
    /// <code>
    /// var currentTime = timeManager.Now;
    /// </code>
    /// </remarks>
    Instant Now { get; }

    /// <summary>
    /// Sets the current time to the specified instant.
    /// </summary>
    /// <param name="now">The instant to set as the current time.</param>
    /// <remarks>
    /// This method is typically used in testing scenarios to control time.
    /// Example usage:
    /// <code>
    /// timeManager.SetNow(SystemClock.Instance.GetCurrentInstant());
    /// </code>
    /// </remarks>
    void SetNow(Instant now);
}
