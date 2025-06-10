using NodaTime;

namespace Annium.NodaTime.Extensions;

/// <summary>
/// Provides extension methods for working with NodaTime Duration objects, including floor, round, and ceiling operations for temporal truncation.
/// </summary>
public static class DurationExtensions
{
    /// <summary>
    /// Floors the duration to the nearest second by truncating any subsecond components.
    /// </summary>
    /// <param name="m">The duration to floor.</param>
    /// <returns>A new duration with subsecond components removed.</returns>
    public static Duration FloorToSecond(this Duration m) => m.FloorTo(Duration.FromSeconds(1));

    /// <summary>
    /// Floors the duration to the nearest minute by truncating any subsecond and second components.
    /// </summary>
    /// <param name="m">The duration to floor.</param>
    /// <returns>A new duration with components smaller than a minute removed.</returns>
    public static Duration FloorToMinute(this Duration m) => m.FloorTo(Duration.FromMinutes(1));

    /// <summary>
    /// Floors the duration to the nearest hour by truncating any components smaller than an hour.
    /// </summary>
    /// <param name="m">The duration to floor.</param>
    /// <returns>A new duration with components smaller than an hour removed.</returns>
    public static Duration FloorToHour(this Duration m) => m.FloorTo(Duration.FromHours(1));

    /// <summary>
    /// Floors the duration to the nearest day by truncating any components smaller than a day.
    /// </summary>
    /// <param name="m">The duration to floor.</param>
    /// <returns>A new duration with components smaller than a day removed.</returns>
    public static Duration FloorToDay(this Duration m) => m.FloorTo(Duration.FromDays(1));

    /// <summary>
    /// Floors the duration to the nearest multiple of the specified duration unit by truncating any remainder.
    /// </summary>
    /// <param name="m">The duration to floor.</param>
    /// <param name="d">The duration unit to floor to.</param>
    /// <returns>A new duration that is the largest multiple of the unit duration that is less than or equal to the input duration.</returns>
    public static Duration FloorTo(this Duration m, Duration d)
    {
        var mt = (long)m.TotalTicks;
        var dt = (long)d.TotalTicks;

        return Duration.FromTicks(mt - mt % dt);
    }

    /// <summary>
    /// Rounds the duration to the nearest second using standard rounding rules.
    /// </summary>
    /// <param name="m">The duration to round.</param>
    /// <returns>A new duration rounded to the nearest second.</returns>
    public static Duration RoundToSecond(this Duration m) => m.RoundTo(Duration.FromSeconds(1));

    /// <summary>
    /// Rounds the duration to the nearest minute using standard rounding rules.
    /// </summary>
    /// <param name="m">The duration to round.</param>
    /// <returns>A new duration rounded to the nearest minute.</returns>
    public static Duration RoundToMinute(this Duration m) => m.RoundTo(Duration.FromMinutes(1));

    /// <summary>
    /// Rounds the duration to the nearest hour using standard rounding rules.
    /// </summary>
    /// <param name="m">The duration to round.</param>
    /// <returns>A new duration rounded to the nearest hour.</returns>
    public static Duration RoundToHour(this Duration m) => m.RoundTo(Duration.FromHours(1));

    /// <summary>
    /// Rounds the duration to the nearest day using standard rounding rules.
    /// </summary>
    /// <param name="m">The duration to round.</param>
    /// <returns>A new duration rounded to the nearest day.</returns>
    public static Duration RoundToDay(this Duration m) => m.RoundTo(Duration.FromDays(1));

    /// <summary>
    /// Rounds the duration to the nearest multiple of the specified duration unit using standard rounding rules.
    /// </summary>
    /// <param name="m">The duration to round.</param>
    /// <param name="d">The duration unit to round to.</param>
    /// <returns>A new duration that is the closest multiple of the unit duration to the input duration.</returns>
    public static Duration RoundTo(this Duration m, Duration d)
    {
        var mt = (long)m.TotalTicks;
        var dt = (long)d.TotalTicks;
        var diff = mt % dt;

        return Duration.FromTicks(mt - diff + (dt > diff * 2L ? 0L : dt));
    }

    /// <summary>
    /// Ceils the duration to the next second by rounding up any subsecond components.
    /// </summary>
    /// <param name="m">The duration to ceil.</param>
    /// <returns>A new duration rounded up to the next second.</returns>
    public static Duration CeilToSecond(this Duration m) => m.CeilTo(Duration.FromSeconds(1));

    /// <summary>
    /// Ceils the duration to the next minute by rounding up any components smaller than a minute.
    /// </summary>
    /// <param name="m">The duration to ceil.</param>
    /// <returns>A new duration rounded up to the next minute.</returns>
    public static Duration CeilToMinute(this Duration m) => m.CeilTo(Duration.FromMinutes(1));

    /// <summary>
    /// Ceils the duration to the next hour by rounding up any components smaller than an hour.
    /// </summary>
    /// <param name="m">The duration to ceil.</param>
    /// <returns>A new duration rounded up to the next hour.</returns>
    public static Duration CeilToHour(this Duration m) => m.CeilTo(Duration.FromHours(1));

    /// <summary>
    /// Ceils the duration to the next day by rounding up any components smaller than a day.
    /// </summary>
    /// <param name="m">The duration to ceil.</param>
    /// <returns>A new duration rounded up to the next day.</returns>
    public static Duration CeilToDay(this Duration m) => m.CeilTo(Duration.FromDays(1));

    /// <summary>
    /// Ceils the duration to the next multiple of the specified duration unit by rounding up any remainder.
    /// </summary>
    /// <param name="m">The duration to ceil.</param>
    /// <param name="d">The duration unit to ceil to.</param>
    /// <returns>A new duration that is the smallest multiple of the unit duration that is greater than or equal to the input duration.</returns>
    public static Duration CeilTo(this Duration m, Duration d)
    {
        var mt = (long)m.TotalTicks;
        var dt = (long)d.TotalTicks;

        return Duration.FromTicks(mt + dt - mt % dt);
    }
}
