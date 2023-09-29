using System;
using NodaTime;

namespace Annium.Core.Runtime.Internal.Time;

internal interface IInternalTimeProvider
{
    Instant Now { get; }
    DateTime DateTimeNow { get; }
    long UnixMsNow { get; }
    long UnixSecondsNow { get; }
}