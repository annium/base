using System;
using NodaTime;

namespace Annium.Core.Runtime.Internal.Time;

/// <summary>
/// Internal implementation of time provider that provides time relative to initialization
/// </summary>
internal class RelativeTimeProvider : IInternalTimeProvider
{
    /// <summary>
    /// The system clock instance
    /// </summary>
    private readonly SystemClock _clock = SystemClock.Instance;

    /// <summary>
    /// The instant when this provider was created
    /// </summary>
    private readonly Instant _since;

    /// <summary>
    /// Initializes a new instance of RelativeTimeProvider with current time as reference
    /// </summary>
    public RelativeTimeProvider()
    {
        _since = _clock.GetCurrentInstant();
    }

    /// <summary>
    /// The current instant relative to the initialization time
    /// </summary>
    public Instant Now => NodaConstants.BclEpoch + (_clock.GetCurrentInstant() - _since);

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
