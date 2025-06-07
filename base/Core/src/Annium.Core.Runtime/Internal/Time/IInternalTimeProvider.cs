using System;
using NodaTime;

namespace Annium.Core.Runtime.Internal.Time;

/// <summary>
/// Internal interface for time providers that supply current time information
/// </summary>
internal interface IInternalTimeProvider
{
    /// <summary>
    /// The current instant in time
    /// </summary>
    Instant Now { get; }

    /// <summary>
    /// The current date and time as UTC DateTime
    /// </summary>
    DateTime DateTimeNow { get; }

    /// <summary>
    /// The current time as Unix timestamp in milliseconds
    /// </summary>
    long UnixMsNow { get; }

    /// <summary>
    /// The current time as Unix timestamp in seconds
    /// </summary>
    long UnixSecondsNow { get; }
}
