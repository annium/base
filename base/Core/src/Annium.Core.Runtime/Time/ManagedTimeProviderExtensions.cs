using NodaTime;

namespace Annium.Core.Runtime.Time;

public static class ManagedTimeProviderExtensions
{
    public static void AddSecond(this ITimeManager timeManager) =>
        timeManager.SetNow(timeManager.Now + Duration.FromSeconds(1L));

    public static void AddSeconds(this ITimeManager timeManager, long seconds) =>
        timeManager.SetNow(timeManager.Now + Duration.FromSeconds(seconds));

    public static void AddMinute(this ITimeManager timeManager) =>
        timeManager.SetNow(timeManager.Now + Duration.FromMinutes(1L));

    public static void AddMinutes(this ITimeManager timeManager, long minutes) =>
        timeManager.SetNow(timeManager.Now + Duration.FromMinutes(minutes));

    public static void AddHour(this ITimeManager timeManager) =>
        timeManager.SetNow(timeManager.Now + Duration.FromHours(1L));

    public static void AddHours(this ITimeManager timeManager, long hours) =>
        timeManager.SetNow(timeManager.Now + Duration.FromHours(hours));

    public static void AddDay(this ITimeManager timeManager) =>
        timeManager.SetNow(timeManager.Now + Duration.FromDays(1L));

    public static void AddDays(this ITimeManager timeManager, long days) =>
        timeManager.SetNow(timeManager.Now + Duration.FromDays(days));
}