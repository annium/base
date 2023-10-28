using System;
using NodaTime;

namespace Annium.Core.Runtime.Internal.Time;

internal class RealTimeProvider : IInternalTimeProvider
{
    private readonly SystemClock _clock = SystemClock.Instance;
    public Instant Now => _clock.GetCurrentInstant();
    public DateTime DateTimeNow => Now.ToDateTimeUtc();
    public long UnixMsNow => Now.ToUnixTimeMilliseconds();
    public long UnixSecondsNow => Now.ToUnixTimeSeconds();
}
