using System;

namespace Annium;

/// <summary>
/// Provides extension methods for <see cref="TimeSpan"/> objects.
/// </summary>
public static class TimeSpanExtensions
{
    /// <summary>
    /// Floors the time span to the nearest second.
    /// </summary>
    /// <param name="m">The time span to floor.</param>
    /// <returns>A time span floored to the nearest second.</returns>
    public static TimeSpan FloorToSecond(this TimeSpan m) => m.FloorTo(TimeSpan.FromSeconds(1));

    /// <summary>
    /// Floors the time span to the nearest minute.
    /// </summary>
    /// <param name="m">The time span to floor.</param>
    /// <returns>A time span floored to the nearest minute.</returns>
    public static TimeSpan FloorToMinute(this TimeSpan m) => m.FloorTo(TimeSpan.FromMinutes(1));

    /// <summary>
    /// Floors the time span to the nearest hour.
    /// </summary>
    /// <param name="m">The time span to floor.</param>
    /// <returns>A time span floored to the nearest hour.</returns>
    public static TimeSpan FloorToHour(this TimeSpan m) => m.FloorTo(TimeSpan.FromHours(1));

    /// <summary>
    /// Floors the time span to the nearest day.
    /// </summary>
    /// <param name="m">The time span to floor.</param>
    /// <returns>A time span floored to the nearest day.</returns>
    public static TimeSpan FloorToDay(this TimeSpan m) => m.FloorTo(TimeSpan.FromDays(1));

    /// <summary>
    /// Floors the time span to the nearest multiple of the specified duration.
    /// </summary>
    /// <param name="t">The time span to floor.</param>
    /// <param name="d">The duration to floor to.</param>
    /// <returns>A time span floored to the nearest multiple of the specified duration.</returns>
    public static TimeSpan FloorTo(this TimeSpan t, TimeSpan d) => TimeSpan.FromTicks(t.Ticks - t.Ticks % d.Ticks);

    /// <summary>
    /// Rounds the time span to the nearest second.
    /// </summary>
    /// <param name="m">The time span to round.</param>
    /// <returns>A time span rounded to the nearest second.</returns>
    public static TimeSpan RoundToSecond(this TimeSpan m) => m.RoundTo(TimeSpan.FromSeconds(1));

    /// <summary>
    /// Rounds the time span to the nearest minute.
    /// </summary>
    /// <param name="m">The time span to round.</param>
    /// <returns>A time span rounded to the nearest minute.</returns>
    public static TimeSpan RoundToMinute(this TimeSpan m) => m.RoundTo(TimeSpan.FromMinutes(1));

    /// <summary>
    /// Rounds the time span to the nearest hour.
    /// </summary>
    /// <param name="m">The time span to round.</param>
    /// <returns>A time span rounded to the nearest hour.</returns>
    public static TimeSpan RoundToHour(this TimeSpan m) => m.RoundTo(TimeSpan.FromHours(1));

    /// <summary>
    /// Rounds the time span to the nearest day.
    /// </summary>
    /// <param name="m">The time span to round.</param>
    /// <returns>A time span rounded to the nearest day.</returns>
    public static TimeSpan RoundToDay(this TimeSpan m) => m.RoundTo(TimeSpan.FromDays(1));

    /// <summary>
    /// Rounds the time span to the nearest multiple of the specified duration.
    /// </summary>
    /// <param name="t">The time span to round.</param>
    /// <param name="d">The duration to round to.</param>
    /// <returns>A time span rounded to the nearest multiple of the specified duration.</returns>
    public static TimeSpan RoundTo(this TimeSpan t, TimeSpan d)
    {
        var mt = t.Ticks;
        var dt = d.Ticks;
        var diff = mt % dt;

        return TimeSpan.FromTicks(mt - diff + (dt > diff * 2L ? 0L : dt));
    }

    /// <summary>
    /// Ceils the time span to the nearest second.
    /// </summary>
    /// <param name="m">The time span to ceil.</param>
    /// <returns>A time span ceiled to the nearest second.</returns>
    public static TimeSpan CeilToSecond(this TimeSpan m) => m.CeilTo(TimeSpan.FromSeconds(1));

    /// <summary>
    /// Ceils the time span to the nearest minute.
    /// </summary>
    /// <param name="m">The time span to ceil.</param>
    /// <returns>A time span ceiled to the nearest minute.</returns>
    public static TimeSpan CeilToMinute(this TimeSpan m) => m.CeilTo(TimeSpan.FromMinutes(1));

    /// <summary>
    /// Ceils the time span to the nearest hour.
    /// </summary>
    /// <param name="m">The time span to ceil.</param>
    /// <returns>A time span ceiled to the nearest hour.</returns>
    public static TimeSpan CeilToHour(this TimeSpan m) => m.CeilTo(TimeSpan.FromHours(1));

    /// <summary>
    /// Ceils the time span to the nearest day.
    /// </summary>
    /// <param name="m">The time span to ceil.</param>
    /// <returns>A time span ceiled to the nearest day.</returns>
    public static TimeSpan CeilToDay(this TimeSpan m) => m.CeilTo(TimeSpan.FromDays(1));

    /// <summary>
    /// Ceils the time span to the nearest multiple of the specified duration.
    /// </summary>
    /// <param name="t">The time span to ceil.</param>
    /// <param name="d">The duration to ceil to.</param>
    /// <returns>A time span ceiled to the nearest multiple of the specified duration.</returns>
    public static TimeSpan CeilTo(this TimeSpan t, TimeSpan d) =>
        TimeSpan.FromTicks(t.Ticks + d.Ticks - t.Ticks % d.Ticks);
}
