using System;

namespace Annium;

/// <summary>
/// Provides extension methods for working with DateTime values.
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// The Unix epoch (January 1, 1970, 00:00:00 UTC).
    /// </summary>
    private static readonly DateTime _unixEpoch = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Converts a Unix time in minutes to a DateTime value.
    /// </summary>
    /// <param name="minutes">The number of minutes since the Unix epoch.</param>
    /// <returns>A DateTime value representing the specified Unix time.</returns>
    public static DateTime FromUnixTimeMinutes(long minutes) => _unixEpoch.AddMinutes(minutes);

    /// <summary>
    /// Converts a Unix time in seconds to a DateTime value.
    /// </summary>
    /// <param name="seconds">The number of seconds since the Unix epoch.</param>
    /// <returns>A DateTime value representing the specified Unix time.</returns>
    public static DateTime FromUnixTimeSeconds(long seconds) => _unixEpoch.AddSeconds(seconds);

    /// <summary>
    /// Converts a Unix time in milliseconds to a DateTime value.
    /// </summary>
    /// <param name="milliseconds">The number of milliseconds since the Unix epoch.</param>
    /// <returns>A DateTime value representing the specified Unix time.</returns>
    public static DateTime FromUnixTimeMilliseconds(long milliseconds) => _unixEpoch.AddMilliseconds(milliseconds);

    /// <summary>
    /// Rounds a DateTime value down to the nearest second.
    /// </summary>
    /// <param name="m">The DateTime value to round.</param>
    /// <returns>A DateTime value rounded down to the nearest second.</returns>
    public static DateTime FloorToSecond(this DateTime m) =>
        DateTime.MinValue + (m - DateTime.MinValue).FloorToSecond();

    /// <summary>
    /// Rounds a DateTime value down to the nearest minute.
    /// </summary>
    /// <param name="m">The DateTime value to round.</param>
    /// <returns>A DateTime value rounded down to the nearest minute.</returns>
    public static DateTime FloorToMinute(this DateTime m) =>
        DateTime.MinValue + (m - DateTime.MinValue).FloorToMinute();

    /// <summary>
    /// Rounds a DateTime value down to the nearest hour.
    /// </summary>
    /// <param name="m">The DateTime value to round.</param>
    /// <returns>A DateTime value rounded down to the nearest hour.</returns>
    public static DateTime FloorToHour(this DateTime m) => DateTime.MinValue + (m - DateTime.MinValue).FloorToHour();

    /// <summary>
    /// Rounds a DateTime value down to the nearest day.
    /// </summary>
    /// <param name="m">The DateTime value to round.</param>
    /// <returns>A DateTime value rounded down to the nearest day.</returns>
    public static DateTime FloorToDay(this DateTime m) => DateTime.MinValue + (m - DateTime.MinValue).FloorToDay();

    /// <summary>
    /// Rounds a DateTime value down to the nearest multiple of the specified time span.
    /// </summary>
    /// <param name="m">The DateTime value to round.</param>
    /// <param name="t">The time span to round to.</param>
    /// <returns>A DateTime value rounded down to the nearest multiple of the specified time span.</returns>
    public static DateTime FloorTo(this DateTime m, TimeSpan t) =>
        DateTime.MinValue + (m - DateTime.MinValue).FloorTo(t);

    /// <summary>
    /// Rounds a DateTime value to the nearest second.
    /// </summary>
    /// <param name="m">The DateTime value to round.</param>
    /// <returns>A DateTime value rounded to the nearest second.</returns>
    public static DateTime RoundToSecond(this DateTime m) =>
        DateTime.MinValue + (m - DateTime.MinValue).RoundToSecond();

    /// <summary>
    /// Rounds a DateTime value to the nearest minute.
    /// </summary>
    /// <param name="m">The DateTime value to round.</param>
    /// <returns>A DateTime value rounded to the nearest minute.</returns>
    public static DateTime RoundToMinute(this DateTime m) =>
        DateTime.MinValue + (m - DateTime.MinValue).RoundToMinute();

    /// <summary>
    /// Rounds a DateTime value to the nearest hour.
    /// </summary>
    /// <param name="m">The DateTime value to round.</param>
    /// <returns>A DateTime value rounded to the nearest hour.</returns>
    public static DateTime RoundToHour(this DateTime m) => DateTime.MinValue + (m - DateTime.MinValue).RoundToHour();

    /// <summary>
    /// Rounds a DateTime value to the nearest day.
    /// </summary>
    /// <param name="m">The DateTime value to round.</param>
    /// <returns>A DateTime value rounded to the nearest day.</returns>
    public static DateTime RoundToDay(this DateTime m) => DateTime.MinValue + (m - DateTime.MinValue).RoundToDay();

    /// <summary>
    /// Rounds a DateTime value to the nearest multiple of the specified time span.
    /// </summary>
    /// <param name="m">The DateTime value to round.</param>
    /// <param name="t">The time span to round to.</param>
    /// <returns>A DateTime value rounded to the nearest multiple of the specified time span.</returns>
    public static DateTime RoundTo(this DateTime m, TimeSpan t) =>
        DateTime.MinValue + (m - DateTime.MinValue).RoundTo(t);

    /// <summary>
    /// Rounds a DateTime value up to the nearest second.
    /// </summary>
    /// <param name="m">The DateTime value to round.</param>
    /// <returns>A DateTime value rounded up to the nearest second.</returns>
    public static DateTime CeilToSecond(this DateTime m) => DateTime.MinValue + (m - DateTime.MinValue).CeilToSecond();

    /// <summary>
    /// Rounds a DateTime value up to the nearest minute.
    /// </summary>
    /// <param name="m">The DateTime value to round.</param>
    /// <returns>A DateTime value rounded up to the nearest minute.</returns>
    public static DateTime CeilToMinute(this DateTime m) => DateTime.MinValue + (m - DateTime.MinValue).CeilToMinute();

    /// <summary>
    /// Rounds a DateTime value up to the nearest hour.
    /// </summary>
    /// <param name="m">The DateTime value to round.</param>
    /// <returns>A DateTime value rounded up to the nearest hour.</returns>
    public static DateTime CeilToHour(this DateTime m) => DateTime.MinValue + (m - DateTime.MinValue).CeilToHour();

    /// <summary>
    /// Rounds a DateTime value up to the nearest day.
    /// </summary>
    /// <param name="m">The DateTime value to round.</param>
    /// <returns>A DateTime value rounded up to the nearest day.</returns>
    public static DateTime CeilToDay(this DateTime m) => DateTime.MinValue + (m - DateTime.MinValue).CeilToDay();

    /// <summary>
    /// Converts a DateTime value to UTC time.
    /// </summary>
    /// <param name="m">The DateTime value to convert.</param>
    /// <returns>A DateTime value in UTC time.</returns>
    public static DateTime InUtc(this DateTime m) => DateTime.SpecifyKind(m, DateTimeKind.Utc);

    /// <summary>
    /// Converts a DateTime value to Unix time in minutes.
    /// </summary>
    /// <param name="m">The DateTime value to convert.</param>
    /// <returns>The number of minutes since the Unix epoch.</returns>
    public static long ToUnixTimeMinutes(this DateTime m) => (m.InUtc() - _unixEpoch).TotalMinutes.FloorInt64();

    /// <summary>
    /// Converts a DateTime value to Unix time in seconds.
    /// </summary>
    /// <param name="m">The DateTime value to convert.</param>
    /// <returns>The number of seconds since the Unix epoch.</returns>
    public static long ToUnixTimeSeconds(this DateTime m) => (m.InUtc() - _unixEpoch).TotalSeconds.FloorInt64();

    /// <summary>
    /// Converts a DateTime value to Unix time in milliseconds.
    /// </summary>
    /// <param name="m">The DateTime value to convert.</param>
    /// <returns>The number of milliseconds since the Unix epoch.</returns>
    public static long ToUnixTimeMilliseconds(this DateTime m) =>
        (m.InUtc() - _unixEpoch).TotalMilliseconds.FloorInt64();
}
