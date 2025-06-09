using System.Runtime.CompilerServices;
using NodaTime;

namespace Annium.NodaTime.Extensions;

/// <summary>
/// Provides extension methods for working with NodaTime LocalDateTime objects, including Unix time conversions, temporal rounding, and utility operations.
/// </summary>
public static class LocalDateTimeExtensions
{
    /// <summary>
    /// Creates a LocalDateTime from Unix time in minutes since the Unix epoch.
    /// </summary>
    /// <param name="minutes">The number of minutes since the Unix epoch (1970-01-01 00:00:00 UTC).</param>
    /// <returns>A LocalDateTime representing the specified Unix time in UTC.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalDateTime FromUnixTimeMinutes(long minutes) =>
        NodaConstants.UnixEpoch.Plus(Duration.FromMinutes(minutes)).InUtc().LocalDateTime;

    /// <summary>
    /// Creates a LocalDateTime from Unix time in seconds since the Unix epoch.
    /// </summary>
    /// <param name="seconds">The number of seconds since the Unix epoch (1970-01-01 00:00:00 UTC).</param>
    /// <returns>A LocalDateTime representing the specified Unix time in UTC.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalDateTime FromUnixTimeSeconds(long seconds) =>
        NodaConstants.UnixEpoch.Plus(Duration.FromSeconds(seconds)).InUtc().LocalDateTime;

    /// <summary>
    /// Creates a LocalDateTime from Unix time in milliseconds since the Unix epoch.
    /// </summary>
    /// <param name="milliseconds">The number of milliseconds since the Unix epoch (1970-01-01 00:00:00 UTC).</param>
    /// <returns>A LocalDateTime representing the specified Unix time in UTC.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalDateTime FromUnixTimeMilliseconds(long milliseconds) =>
        NodaConstants.UnixEpoch.Plus(Duration.FromMilliseconds(milliseconds)).InUtc().LocalDateTime;

    /// <summary>
    /// Extracts the year and month components from the LocalDateTime as a YearMonth.
    /// </summary>
    /// <param name="m">The LocalDateTime to extract from.</param>
    /// <returns>A YearMonth representing the year and month of the LocalDateTime.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static YearMonth GetYearMonth(this LocalDateTime m) => new(m.Era, m.YearOfEra, m.Month, m.Calendar);

    /// <summary>
    /// Determines whether the LocalDateTime represents midnight (00:00:00.000).
    /// </summary>
    /// <param name="dateTime">The LocalDateTime to check.</param>
    /// <returns>True if the LocalDateTime represents midnight; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsMidnight(this LocalDateTime dateTime) =>
        dateTime is { Hour: 0, Minute: 0 } and { Second: 0, Millisecond: 0 };

    /// <summary>
    /// Converts the LocalDateTime to Unix time in minutes since the Unix epoch, treating it as UTC.
    /// </summary>
    /// <param name="m">The LocalDateTime to convert.</param>
    /// <returns>The number of minutes since the Unix epoch (1970-01-01 00:00:00 UTC).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ToUnixTimeMinutes(this LocalDateTime m) =>
        m.InUtc().ToInstant().Minus(NodaConstants.UnixEpoch).TotalMinutes.FloorInt64();

    /// <summary>
    /// Converts the LocalDateTime to Unix time in seconds since the Unix epoch, treating it as UTC.
    /// </summary>
    /// <param name="m">The LocalDateTime to convert.</param>
    /// <returns>The number of seconds since the Unix epoch (1970-01-01 00:00:00 UTC).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ToUnixTimeSeconds(this LocalDateTime m) =>
        m.InUtc().ToInstant().Minus(NodaConstants.UnixEpoch).TotalSeconds.FloorInt64();

    /// <summary>
    /// Converts the LocalDateTime to Unix time in milliseconds since the Unix epoch, treating it as UTC.
    /// </summary>
    /// <param name="m">The LocalDateTime to convert.</param>
    /// <returns>The number of milliseconds since the Unix epoch (1970-01-01 00:00:00 UTC).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ToUnixTimeMilliseconds(this LocalDateTime m) =>
        m.InUtc().ToInstant().Minus(NodaConstants.UnixEpoch).TotalMilliseconds.FloorInt64();

    /// <summary>
    /// Floors the LocalDateTime to the nearest second by truncating any subsecond components.
    /// </summary>
    /// <param name="m">The LocalDateTime to floor.</param>
    /// <returns>A new LocalDateTime with subsecond components removed.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalDateTime FloorToSecond(this LocalDateTime m) => m.FloorTo(Period.FromSeconds(1));

    /// <summary>
    /// Floors the LocalDateTime to the nearest minute by truncating any subsecond and second components.
    /// </summary>
    /// <param name="m">The LocalDateTime to floor.</param>
    /// <returns>A new LocalDateTime with components smaller than a minute removed.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalDateTime FloorToMinute(this LocalDateTime m) => m.FloorTo(Period.FromMinutes(1));

    /// <summary>
    /// Floors the LocalDateTime to the nearest hour by truncating any components smaller than an hour.
    /// </summary>
    /// <param name="m">The LocalDateTime to floor.</param>
    /// <returns>A new LocalDateTime with components smaller than an hour removed.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalDateTime FloorToHour(this LocalDateTime m) => m.FloorTo(Period.FromHours(1));

    /// <summary>
    /// Floors the LocalDateTime to the nearest day by truncating any components smaller than a day.
    /// </summary>
    /// <param name="m">The LocalDateTime to floor.</param>
    /// <returns>A new LocalDateTime with components smaller than a day removed.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalDateTime FloorToDay(this LocalDateTime m) => m.FloorTo(Period.FromDays(1));

    /// <summary>
    /// Floors the LocalDateTime to the nearest multiple of the specified period by truncating any remainder.
    /// </summary>
    /// <param name="m">The LocalDateTime to floor.</param>
    /// <param name="p">The period unit to floor to.</param>
    /// <returns>A new LocalDateTime that is the largest multiple of the unit period that is less than or equal to the input LocalDateTime.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalDateTime FloorTo(this LocalDateTime m, Period p) =>
        m.InUtc().ToInstant().FloorTo(p.ToDuration()).InUtc().LocalDateTime;

    /// <summary>
    /// Rounds the LocalDateTime to the nearest second using standard rounding rules.
    /// </summary>
    /// <param name="m">The LocalDateTime to round.</param>
    /// <returns>A new LocalDateTime rounded to the nearest second.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalDateTime RoundToSecond(this LocalDateTime m) => m.RoundTo(Period.FromSeconds(1));

    /// <summary>
    /// Rounds the LocalDateTime to the nearest minute using standard rounding rules.
    /// </summary>
    /// <param name="m">The LocalDateTime to round.</param>
    /// <returns>A new LocalDateTime rounded to the nearest minute.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalDateTime RoundToMinute(this LocalDateTime m) => m.RoundTo(Period.FromMinutes(1));

    /// <summary>
    /// Rounds the LocalDateTime to the nearest hour using standard rounding rules.
    /// </summary>
    /// <param name="m">The LocalDateTime to round.</param>
    /// <returns>A new LocalDateTime rounded to the nearest hour.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalDateTime RoundToHour(this LocalDateTime m) => m.RoundTo(Period.FromHours(1));

    /// <summary>
    /// Rounds the LocalDateTime to the nearest day using standard rounding rules.
    /// </summary>
    /// <param name="m">The LocalDateTime to round.</param>
    /// <returns>A new LocalDateTime rounded to the nearest day.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalDateTime RoundToDay(this LocalDateTime m) => m.RoundTo(Period.FromDays(1));

    /// <summary>
    /// Rounds the LocalDateTime to the nearest multiple of the specified period using standard rounding rules.
    /// </summary>
    /// <param name="m">The LocalDateTime to round.</param>
    /// <param name="p">The period unit to round to.</param>
    /// <returns>A new LocalDateTime that is the closest multiple of the unit period to the input LocalDateTime.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalDateTime RoundTo(this LocalDateTime m, Period p) =>
        m.InUtc().ToInstant().RoundTo(p.ToDuration()).InUtc().LocalDateTime;

    /// <summary>
    /// Ceils the LocalDateTime to the next second by rounding up any subsecond components.
    /// </summary>
    /// <param name="m">The LocalDateTime to ceil.</param>
    /// <returns>A new LocalDateTime rounded up to the next second.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalDateTime CeilToSecond(this LocalDateTime m) => m.CeilTo(Period.FromSeconds(1));

    /// <summary>
    /// Ceils the LocalDateTime to the next minute by rounding up any components smaller than a minute.
    /// </summary>
    /// <param name="m">The LocalDateTime to ceil.</param>
    /// <returns>A new LocalDateTime rounded up to the next minute.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalDateTime CeilToMinute(this LocalDateTime m) => m.CeilTo(Period.FromMinutes(1));

    /// <summary>
    /// Ceils the LocalDateTime to the next hour by rounding up any components smaller than an hour.
    /// </summary>
    /// <param name="m">The LocalDateTime to ceil.</param>
    /// <returns>A new LocalDateTime rounded up to the next hour.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalDateTime CeilToHour(this LocalDateTime m) => m.CeilTo(Period.FromHours(1));

    /// <summary>
    /// Ceils the LocalDateTime to the next day by rounding up any components smaller than a day.
    /// </summary>
    /// <param name="m">The LocalDateTime to ceil.</param>
    /// <returns>A new LocalDateTime rounded up to the next day.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalDateTime CeilToDay(this LocalDateTime m) => m.CeilTo(Period.FromDays(1));

    /// <summary>
    /// Ceils the LocalDateTime to the next multiple of the specified period by rounding up any remainder.
    /// </summary>
    /// <param name="m">The LocalDateTime to ceil.</param>
    /// <param name="p">The period unit to ceil to.</param>
    /// <returns>A new LocalDateTime that is the smallest multiple of the unit period that is greater than or equal to the input LocalDateTime.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalDateTime CeilTo(this LocalDateTime m, Period p) =>
        m.InUtc().ToInstant().CeilTo(p.ToDuration()).InUtc().LocalDateTime;
}
