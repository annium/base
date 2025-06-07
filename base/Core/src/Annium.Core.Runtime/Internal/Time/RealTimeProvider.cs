using System;
using NodaTime;

namespace Annium.Core.Runtime.Internal.Time;

/// <summary>
/// Internal implementation of time provider that uses real system time
/// </summary>
internal class RealTimeProvider : IInternalTimeProvider
{
    /// <summary>
    /// The system clock instance
    /// </summary>
    private readonly SystemClock _clock = SystemClock.Instance;

    /// <summary>
    /// The current instant in time from system clock
    /// </summary>
    public Instant Now => _clock.GetCurrentInstant();

    /// <summary>
    /// The current date and time as UTC DateTime
    /// </summary>
    public DateTime DateTimeNow => Now.ToDateTimeUtc();

    /// <summary>
    /// The current time as Unix timestamp in milliseconds
    /// </summary>
    public long UnixMsNow => Now.ToUnixTimeMilliseconds();

    /// <summary>
    /// The current time as Unix timestamp in seconds
    /// </summary>
    public long UnixSecondsNow => Now.ToUnixTimeSeconds();
}
