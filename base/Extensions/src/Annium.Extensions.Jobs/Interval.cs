namespace Annium.Extensions.Jobs;

/// <summary>
/// Provides predefined cron expressions for common scheduling intervals
/// </summary>
public static class Interval
{
    /// <summary>
    /// Every second cron expression
    /// </summary>
    public const string Secondly = "* * * * *";

    /// <summary>
    /// Every minute cron expression
    /// </summary>
    public const string Minutely = "0 * * * *";

    /// <summary>
    /// Every hour cron expression
    /// </summary>
    public const string Hourly = "0 0 * * *";

    /// <summary>
    /// Every day cron expression
    /// </summary>
    public const string Daily = "0 0 0 * *";

    /// <summary>
    /// Creates a cron expression for every N seconds
    /// </summary>
    /// <param name="seconds">The number of seconds</param>
    /// <returns>The cron expression</returns>
    public static string InSeconds(uint seconds) => $"*/{seconds} * * * *";

    /// <summary>
    /// Creates a cron expression for every N minutes
    /// </summary>
    /// <param name="minutes">The number of minutes</param>
    /// <returns>The cron expression</returns>
    public static string InMinutes(uint minutes) => $"0 */{minutes} * * *";

    /// <summary>
    /// Creates a cron expression for every N hours
    /// </summary>
    /// <param name="hours">The number of hours</param>
    /// <returns>The cron expression</returns>
    public static string InHours(uint hours) => $"0 0 */{hours} * *";
}
