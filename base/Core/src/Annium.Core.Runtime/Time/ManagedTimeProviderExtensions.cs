using NodaTime;

namespace Annium.Core.Runtime.Time;

/// <summary>
/// Extension methods for ITimeManager to simplify time manipulation operations
/// </summary>
public static class ManagedTimeProviderExtensions
{
    /// <summary>
    /// Advances the managed time by one second
    /// </summary>
    /// <param name="timeManager">The time manager to advance</param>
    public static void AddSecond(this ITimeManager timeManager) =>
        timeManager.SetNow(timeManager.Now + Duration.FromSeconds(1L));

    /// <summary>
    /// Advances the managed time by the specified number of seconds
    /// </summary>
    /// <param name="timeManager">The time manager to advance</param>
    /// <param name="seconds">The number of seconds to add</param>
    public static void AddSeconds(this ITimeManager timeManager, long seconds) =>
        timeManager.SetNow(timeManager.Now + Duration.FromSeconds(seconds));

    /// <summary>
    /// Advances the managed time by one minute
    /// </summary>
    /// <param name="timeManager">The time manager to advance</param>
    public static void AddMinute(this ITimeManager timeManager) =>
        timeManager.SetNow(timeManager.Now + Duration.FromMinutes(1L));

    /// <summary>
    /// Advances the managed time by the specified number of minutes
    /// </summary>
    /// <param name="timeManager">The time manager to advance</param>
    /// <param name="minutes">The number of minutes to add</param>
    public static void AddMinutes(this ITimeManager timeManager, long minutes) =>
        timeManager.SetNow(timeManager.Now + Duration.FromMinutes(minutes));

    /// <summary>
    /// Advances the managed time by one hour
    /// </summary>
    /// <param name="timeManager">The time manager to advance</param>
    public static void AddHour(this ITimeManager timeManager) =>
        timeManager.SetNow(timeManager.Now + Duration.FromHours(1L));

    /// <summary>
    /// Advances the managed time by the specified number of hours
    /// </summary>
    /// <param name="timeManager">The time manager to advance</param>
    /// <param name="hours">The number of hours to add</param>
    public static void AddHours(this ITimeManager timeManager, long hours) =>
        timeManager.SetNow(timeManager.Now + Duration.FromHours(hours));

    /// <summary>
    /// Advances the managed time by one day
    /// </summary>
    /// <param name="timeManager">The time manager to advance</param>
    public static void AddDay(this ITimeManager timeManager) =>
        timeManager.SetNow(timeManager.Now + Duration.FromDays(1L));

    /// <summary>
    /// Advances the managed time by the specified number of days
    /// </summary>
    /// <param name="timeManager">The time manager to advance</param>
    /// <param name="days">The number of days to add</param>
    public static void AddDays(this ITimeManager timeManager, long days) =>
        timeManager.SetNow(timeManager.Now + Duration.FromDays(days));
}
