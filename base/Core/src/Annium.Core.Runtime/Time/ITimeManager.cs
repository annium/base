using System;
using NodaTime;

namespace Annium.Core.Runtime.Time;

public interface ITimeManager
{
    event Action<Duration> NowChanged;
    Instant Now { get; }
    void SetNow(Instant now);
}