using System;
using NodaTime;

namespace Annium.Core.Runtime.Internal.Time;

internal class RelativeTimeProvider : IInternalTimeProvider
{
    private readonly SystemClock _clock = SystemClock.Instance;
    private readonly Instant _since;

    public RelativeTimeProvider()
    {
        _since = _clock.GetCurrentInstant();
    }

    public Instant Now => NodaConstants.BclEpoch + (_clock.GetCurrentInstant() - _since);
    public DateTime DateTimeNow => Now.ToDateTimeUtc();
    public long UnixMsNow => Now.ToUnixTimeMilliseconds();
    public long UnixSecondsNow => Now.ToUnixTimeSeconds();
}