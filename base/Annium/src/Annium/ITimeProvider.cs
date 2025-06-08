using System;
using NodaTime;

namespace Annium;

/// <summary>
/// Provides access to the current time in various formats.
/// </summary>
public interface ITimeProvider
{
    /// <summary>
    /// Gets the current instant in time.
    /// </summary>
    Instant Now { get; }

    /// <summary>
    /// Gets the current date and time in the local time zone.
    /// </summary>
    DateTime DateTimeNow { get; }

    /// <summary>
    /// Gets the current Unix timestamp in milliseconds.
    /// </summary>
    long UnixMsNow { get; }

    /// <summary>
    /// Gets the current Unix timestamp in seconds.
    /// </summary>
    long UnixSecondsNow { get; }
}
