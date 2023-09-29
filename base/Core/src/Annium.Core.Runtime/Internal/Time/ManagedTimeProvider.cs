using System;
using Annium.Core.Runtime.Time;
using NodaTime;

namespace Annium.Core.Runtime.Internal.Time;

internal class ManagedTimeProvider : ITimeManager, IInternalTimeProvider
{
    public event Action<Duration> NowChanged = delegate { };
    public Instant Now { get; private set; }
    public DateTime DateTimeNow { get; private set; }
    public long UnixMsNow { get; private set; }
    public long UnixSecondsNow { get; private set; }

    public void SetNow(Instant now)
    {
        var duration = now - Now;
        Now = now;
        DateTimeNow = now.ToDateTimeUtc();
        UnixMsNow = now.ToUnixTimeMilliseconds();
        UnixSecondsNow = now.ToUnixTimeSeconds();
        NowChanged(duration);
    }
}