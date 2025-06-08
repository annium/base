using System;
using NodaTime;

namespace Annium.Extensions.Jobs.Internal;

/// <summary>
/// Resolves cron expressions into time matchers for determining if a given instant matches the schedule
/// </summary>
internal interface IIntervalResolver
{
    /// <summary>
    /// Creates a function that determines if a given instant matches the cron expression
    /// </summary>
    /// <param name="interval">The cron expression (format: "minute hour day month dayOfWeek")</param>
    /// <returns>A function that takes an instant and returns true if it matches the schedule</returns>
    Func<Instant, bool> GetMatcher(string interval);
}
