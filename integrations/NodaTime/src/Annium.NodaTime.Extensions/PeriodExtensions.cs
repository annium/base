using NodaTime;

namespace Annium.NodaTime.Extensions;

/// <summary>
/// Provides extension methods for working with NodaTime Period objects, including floor, round, and ceiling operations for temporal truncation.
/// </summary>
public static class PeriodExtensions
{
    /// <summary>
    /// Floors the period to the nearest second by truncating any subsecond components.
    /// </summary>
    /// <param name="m">The period to floor.</param>
    /// <returns>A new period with subsecond components removed.</returns>
    public static Period FloorToSecond(this Period m) => m.FloorTo(Period.FromSeconds(1));

    /// <summary>
    /// Floors the period to the nearest minute by truncating any subsecond and second components.
    /// </summary>
    /// <param name="m">The period to floor.</param>
    /// <returns>A new period with components smaller than a minute removed.</returns>
    public static Period FloorToMinute(this Period m) => m.FloorTo(Period.FromMinutes(1));

    /// <summary>
    /// Floors the period to the nearest hour by truncating any components smaller than an hour.
    /// </summary>
    /// <param name="m">The period to floor.</param>
    /// <returns>A new period with components smaller than an hour removed.</returns>
    public static Period FloorToHour(this Period m) => m.FloorTo(Period.FromHours(1));

    /// <summary>
    /// Floors the period to the nearest day by truncating any components smaller than a day.
    /// </summary>
    /// <param name="m">The period to floor.</param>
    /// <returns>A new period with components smaller than a day removed.</returns>
    public static Period FloorToDay(this Period m) => m.FloorTo(Period.FromDays(1));

    /// <summary>
    /// Floors the period to the nearest multiple of the specified period unit by truncating any remainder.
    /// </summary>
    /// <param name="m">The period to floor.</param>
    /// <param name="d">The period unit to floor to.</param>
    /// <returns>A new period that is the largest multiple of the unit period that is less than or equal to the input period.</returns>
    public static Period FloorTo(this Period m, Period d)
    {
        var mt = (long)m.ToDuration().TotalTicks;
        var dt = (long)d.ToDuration().TotalTicks;

        return Period.FromTicks(mt - mt % dt);
    }

    /// <summary>
    /// Rounds the period to the nearest second using standard rounding rules.
    /// </summary>
    /// <param name="m">The period to round.</param>
    /// <returns>A new period rounded to the nearest second.</returns>
    public static Period RoundToSecond(this Period m) => m.RoundTo(Period.FromSeconds(1));

    /// <summary>
    /// Rounds the period to the nearest minute using standard rounding rules.
    /// </summary>
    /// <param name="m">The period to round.</param>
    /// <returns>A new period rounded to the nearest minute.</returns>
    public static Period RoundToMinute(this Period m) => m.RoundTo(Period.FromMinutes(1));

    /// <summary>
    /// Rounds the period to the nearest hour using standard rounding rules.
    /// </summary>
    /// <param name="m">The period to round.</param>
    /// <returns>A new period rounded to the nearest hour.</returns>
    public static Period RoundToHour(this Period m) => m.RoundTo(Period.FromHours(1));

    /// <summary>
    /// Rounds the period to the nearest day using standard rounding rules.
    /// </summary>
    /// <param name="m">The period to round.</param>
    /// <returns>A new period rounded to the nearest day.</returns>
    public static Period RoundToDay(this Period m) => m.RoundTo(Period.FromDays(1));

    /// <summary>
    /// Rounds the period to the nearest multiple of the specified period unit using standard rounding rules.
    /// </summary>
    /// <param name="m">The period to round.</param>
    /// <param name="d">The period unit to round to.</param>
    /// <returns>A new period that is the closest multiple of the unit period to the input period.</returns>
    public static Period RoundTo(this Period m, Period d)
    {
        var mt = (long)m.ToDuration().TotalTicks;
        var dt = (long)d.ToDuration().TotalTicks;
        var diff = mt % dt;

        return Period.FromTicks(mt - diff + (dt > diff * 2L ? 0L : dt));
    }

    /// <summary>
    /// Ceils the period to the next second by rounding up any subsecond components.
    /// </summary>
    /// <param name="m">The period to ceil.</param>
    /// <returns>A new period rounded up to the next second.</returns>
    public static Period CeilToSecond(this Period m) => m.CeilTo(Period.FromSeconds(1));

    /// <summary>
    /// Ceils the period to the next minute by rounding up any components smaller than a minute.
    /// </summary>
    /// <param name="m">The period to ceil.</param>
    /// <returns>A new period rounded up to the next minute.</returns>
    public static Period CeilToMinute(this Period m) => m.CeilTo(Period.FromMinutes(1));

    /// <summary>
    /// Ceils the period to the next hour by rounding up any components smaller than an hour.
    /// </summary>
    /// <param name="m">The period to ceil.</param>
    /// <returns>A new period rounded up to the next hour.</returns>
    public static Period CeilToHour(this Period m) => m.CeilTo(Period.FromHours(1));

    /// <summary>
    /// Ceils the period to the next day by rounding up any components smaller than a day.
    /// </summary>
    /// <param name="m">The period to ceil.</param>
    /// <returns>A new period rounded up to the next day.</returns>
    public static Period CeilToDay(this Period m) => m.CeilTo(Period.FromDays(1));

    /// <summary>
    /// Ceils the period to the next multiple of the specified period unit by rounding up any remainder.
    /// </summary>
    /// <param name="m">The period to ceil.</param>
    /// <param name="d">The period unit to ceil to.</param>
    /// <returns>A new period that is the smallest multiple of the unit period that is greater than or equal to the input period.</returns>
    public static Period CeilTo(this Period m, Period d)
    {
        var mt = (long)m.ToDuration().TotalTicks;
        var dt = (long)d.ToDuration().TotalTicks;

        return Period.FromTicks(mt + dt - mt % dt);
    }
}
