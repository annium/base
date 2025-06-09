using System.Runtime.CompilerServices;
using NodaTime;

namespace Annium.NodaTime.Extensions;

/// <summary>
/// Provides extension methods for working with NodaTime ZonedDateTime objects, including Unix time conversions, temporal rounding, and utility operations.
/// </summary>
public static class ZonedDateTimeExtensions
{
    /// <summary>
    /// Creates a ZonedDateTime from Unix time in minutes since the Unix epoch.
    /// </summary>
    /// <param name="minutes">The number of minutes since the Unix epoch (1970-01-01 00:00:00 UTC).</param>
    /// <returns>A ZonedDateTime representing the specified Unix time in UTC.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ZonedDateTime FromUnixTimeMinutes(long minutes) =>
        NodaConstants.UnixEpoch.Plus(Duration.FromMinutes(minutes)).InUtc();

    /// <summary>
    /// Creates a ZonedDateTime from Unix time in seconds since the Unix epoch.
    /// </summary>
    /// <param name="seconds">The number of seconds since the Unix epoch (1970-01-01 00:00:00 UTC).</param>
    /// <returns>A ZonedDateTime representing the specified Unix time in UTC.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ZonedDateTime FromUnixTimeSeconds(long seconds) =>
        NodaConstants.UnixEpoch.Plus(Duration.FromSeconds(seconds)).InUtc();

    /// <summary>
    /// Creates a ZonedDateTime from Unix time in milliseconds since the Unix epoch.
    /// </summary>
    /// <param name="milliseconds">The number of milliseconds since the Unix epoch (1970-01-01 00:00:00 UTC).</param>
    /// <returns>A ZonedDateTime representing the specified Unix time in UTC.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ZonedDateTime FromUnixTimeMilliseconds(long milliseconds) =>
        NodaConstants.UnixEpoch.Plus(Duration.FromMilliseconds(milliseconds)).InUtc();

    /// <summary>
    /// Extracts the year and month components from the ZonedDateTime as a YearMonth.
    /// </summary>
    /// <param name="m">The ZonedDateTime to extract from.</param>
    /// <returns>A YearMonth representing the year and month of the ZonedDateTime.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static YearMonth GetYearMonth(this ZonedDateTime m) => new(m.Era, m.YearOfEra, m.Month, m.Calendar);

    /// <summary>
    /// Converts the ZonedDateTime to Unix time in minutes since the Unix epoch.
    /// </summary>
    /// <param name="m">The ZonedDateTime to convert.</param>
    /// <returns>The number of minutes since the Unix epoch (1970-01-01 00:00:00 UTC).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ToUnixTimeMinutes(this ZonedDateTime m) =>
        m.ToInstant().Minus(NodaConstants.UnixEpoch).TotalMinutes.FloorInt64();

    /// <summary>
    /// Converts the ZonedDateTime to Unix time in seconds since the Unix epoch.
    /// </summary>
    /// <param name="m">The ZonedDateTime to convert.</param>
    /// <returns>The number of seconds since the Unix epoch (1970-01-01 00:00:00 UTC).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ToUnixTimeSeconds(this ZonedDateTime m) =>
        m.ToInstant().Minus(NodaConstants.UnixEpoch).TotalSeconds.FloorInt64();

    /// <summary>
    /// Converts the ZonedDateTime to Unix time in milliseconds since the Unix epoch.
    /// </summary>
    /// <param name="m">The ZonedDateTime to convert.</param>
    /// <returns>The number of milliseconds since the Unix epoch (1970-01-01 00:00:00 UTC).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ToUnixTimeMilliseconds(this ZonedDateTime m) =>
        m.ToInstant().Minus(NodaConstants.UnixEpoch).TotalMilliseconds.FloorInt64();

    /// <summary>
    /// Floors the ZonedDateTime to the nearest second by truncating any subsecond components.
    /// </summary>
    /// <param name="m">The ZonedDateTime to floor.</param>
    /// <returns>A new ZonedDateTime with subsecond components removed.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ZonedDateTime FloorToSecond(this ZonedDateTime m) => m.FloorTo(Period.FromSeconds(1));

    /// <summary>
    /// Floors the ZonedDateTime to the nearest minute by truncating any subsecond and second components.
    /// </summary>
    /// <param name="m">The ZonedDateTime to floor.</param>
    /// <returns>A new ZonedDateTime with components smaller than a minute removed.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ZonedDateTime FloorToMinute(this ZonedDateTime m) => m.FloorTo(Period.FromMinutes(1));

    /// <summary>
    /// Floors the ZonedDateTime to the nearest hour by truncating any components smaller than an hour.
    /// </summary>
    /// <param name="m">The ZonedDateTime to floor.</param>
    /// <returns>A new ZonedDateTime with components smaller than an hour removed.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ZonedDateTime FloorToHour(this ZonedDateTime m) => m.FloorTo(Period.FromHours(1));

    /// <summary>
    /// Floors the ZonedDateTime to the nearest day by truncating any components smaller than a day.
    /// </summary>
    /// <param name="m">The ZonedDateTime to floor.</param>
    /// <returns>A new ZonedDateTime with components smaller than a day removed.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ZonedDateTime FloorToDay(this ZonedDateTime m) => m.FloorTo(Period.FromDays(1));

    /// <summary>
    /// Floors the ZonedDateTime to the nearest multiple of the specified period by truncating any remainder.
    /// </summary>
    /// <param name="m">The ZonedDateTime to floor.</param>
    /// <param name="p">The period unit to floor to.</param>
    /// <returns>A new ZonedDateTime that is the largest multiple of the unit period that is less than or equal to the input ZonedDateTime.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ZonedDateTime FloorTo(this ZonedDateTime m, Period p) =>
        new(m.ToInstant().FloorTo(p.ToDuration()), m.Zone, m.Calendar);

    /// <summary>
    /// Rounds the ZonedDateTime to the nearest second using standard rounding rules.
    /// </summary>
    /// <param name="m">The ZonedDateTime to round.</param>
    /// <returns>A new ZonedDateTime rounded to the nearest second.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ZonedDateTime RoundToSecond(this ZonedDateTime m) => m.RoundTo(Period.FromSeconds(1));

    /// <summary>
    /// Rounds the ZonedDateTime to the nearest minute using standard rounding rules.
    /// </summary>
    /// <param name="m">The ZonedDateTime to round.</param>
    /// <returns>A new ZonedDateTime rounded to the nearest minute.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ZonedDateTime RoundToMinute(this ZonedDateTime m) => m.RoundTo(Period.FromMinutes(1));

    /// <summary>
    /// Rounds the ZonedDateTime to the nearest hour using standard rounding rules.
    /// </summary>
    /// <param name="m">The ZonedDateTime to round.</param>
    /// <returns>A new ZonedDateTime rounded to the nearest hour.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ZonedDateTime RoundToHour(this ZonedDateTime m) => m.RoundTo(Period.FromHours(1));

    /// <summary>
    /// Rounds the ZonedDateTime to the nearest day using standard rounding rules.
    /// </summary>
    /// <param name="m">The ZonedDateTime to round.</param>
    /// <returns>A new ZonedDateTime rounded to the nearest day.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ZonedDateTime RoundToDay(this ZonedDateTime m) => m.RoundTo(Period.FromDays(1));

    /// <summary>
    /// Rounds the ZonedDateTime to the nearest multiple of the specified period using standard rounding rules.
    /// </summary>
    /// <param name="m">The ZonedDateTime to round.</param>
    /// <param name="p">The period unit to round to.</param>
    /// <returns>A new ZonedDateTime that is the closest multiple of the unit period to the input ZonedDateTime.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ZonedDateTime RoundTo(this ZonedDateTime m, Period p) =>
        new(m.ToInstant().RoundTo(p.ToDuration()), m.Zone, m.Calendar);

    /// <summary>
    /// Ceils the ZonedDateTime to the next second by rounding up any subsecond components.
    /// </summary>
    /// <param name="m">The ZonedDateTime to ceil.</param>
    /// <returns>A new ZonedDateTime rounded up to the next second.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ZonedDateTime CeilToSecond(this ZonedDateTime m) => m.CeilTo(Period.FromSeconds(1));

    /// <summary>
    /// Ceils the ZonedDateTime to the next minute by rounding up any components smaller than a minute.
    /// </summary>
    /// <param name="m">The ZonedDateTime to ceil.</param>
    /// <returns>A new ZonedDateTime rounded up to the next minute.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ZonedDateTime CeilToMinute(this ZonedDateTime m) => m.CeilTo(Period.FromMinutes(1));

    /// <summary>
    /// Ceils the ZonedDateTime to the next hour by rounding up any components smaller than an hour.
    /// </summary>
    /// <param name="m">The ZonedDateTime to ceil.</param>
    /// <returns>A new ZonedDateTime rounded up to the next hour.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ZonedDateTime CeilToHour(this ZonedDateTime m) => m.CeilTo(Period.FromHours(1));

    /// <summary>
    /// Ceils the ZonedDateTime to the next day by rounding up any components smaller than a day.
    /// </summary>
    /// <param name="m">The ZonedDateTime to ceil.</param>
    /// <returns>A new ZonedDateTime rounded up to the next day.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ZonedDateTime CeilToDay(this ZonedDateTime m) => m.CeilTo(Period.FromDays(1));

    /// <summary>
    /// Ceils the ZonedDateTime to the next multiple of the specified period by rounding up any remainder.
    /// </summary>
    /// <param name="m">The ZonedDateTime to ceil.</param>
    /// <param name="p">The period unit to ceil to.</param>
    /// <returns>A new ZonedDateTime that is the smallest multiple of the unit period that is greater than or equal to the input ZonedDateTime.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ZonedDateTime CeilTo(this ZonedDateTime m, Period p) =>
        new(m.ToInstant().CeilTo(p.ToDuration()), m.Zone, m.Calendar);

    /// <summary>
    /// Determines whether the ZonedDateTime represents midnight (00:00:00.000) in its time zone.
    /// </summary>
    /// <param name="dateTime">The ZonedDateTime to check.</param>
    /// <returns>True if the ZonedDateTime represents midnight; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsMidnight(this ZonedDateTime dateTime) =>
        dateTime is { Hour: 0, Minute: 0 } and { Second: 0, Millisecond: 0 };
}
