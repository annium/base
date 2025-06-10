using System;
using Annium.Core.Runtime.Time;
using NodaTime;

namespace Annium.Core.Runtime.Internal.Time;

/// <summary>
/// Internal implementation of managed time provider that allows manual time control
/// </summary>
internal class ManagedTimeProvider : ITimeManager, IInternalTimeProvider
{
    /// <summary>
    /// Event fired when the current time changes
    /// </summary>
    public event Action<Duration> OnNowChanged = delegate { };

    /// <summary>
    /// The current instant in time
    /// </summary>
    public Instant Now { get; private set; }

    /// <summary>
    /// The current date and time as UTC DateTime
    /// </summary>
    public DateTime DateTimeNow { get; private set; }

    /// <summary>
    /// The current time as Unix timestamp in milliseconds
    /// </summary>
    public long UnixMsNow { get; private set; }

    /// <summary>
    /// The current time as Unix timestamp in seconds
    /// </summary>
    public long UnixSecondsNow { get; private set; }

    /// <summary>
    /// Sets the current time to the specified instant
    /// </summary>
    /// <param name="now">The instant to set as current time</param>
    public void SetNow(Instant now)
    {
        var duration = now - Now;
        Now = now;
        DateTimeNow = now.ToDateTimeUtc();
        UnixMsNow = now.ToUnixTimeMilliseconds();
        UnixSecondsNow = now.ToUnixTimeSeconds();
        OnNowChanged(duration);
    }
}
