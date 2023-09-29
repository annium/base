using System;
using NodaTime;

namespace Annium.Extensions.Jobs.Internal;

public interface IIntervalParser
{
    Func<LocalDateTime, Duration> GetDelayResolver(string interval);
}