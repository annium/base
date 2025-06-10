using System;
using NodaTime;

namespace Annium.Extensions.Jobs.Internal;

/// <summary>
/// Parses cron expressions and creates delay resolvers for job scheduling
/// </summary>
public interface IIntervalParser
{
    /// <summary>
    /// Creates a function that calculates the delay until the next scheduled execution
    /// </summary>
    /// <param name="interval">The cron expression (format: "second minute hour day day-of-week")</param>
    /// <returns>A function that takes the current time and returns the delay until next execution</returns>
    Func<LocalDateTime, Duration> GetDelayResolver(string interval);
}
