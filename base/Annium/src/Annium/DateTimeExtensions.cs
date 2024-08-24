using System;

namespace Annium;

public static class DateTimeExtensions
{
    private static readonly DateTime _unixEpoch = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

    public static DateTime FromUnixTimeMinutes(long minutes) => _unixEpoch.AddMinutes(minutes);

    public static DateTime FromUnixTimeSeconds(long seconds) => _unixEpoch.AddSeconds(seconds);

    public static DateTime FromUnixTimeMilliseconds(long milliseconds) => _unixEpoch.AddMilliseconds(milliseconds);

    public static DateTime FloorToSecond(this DateTime m) =>
        DateTime.MinValue + (m - DateTime.MinValue).FloorToSecond();

    public static DateTime FloorToMinute(this DateTime m) =>
        DateTime.MinValue + (m - DateTime.MinValue).FloorToMinute();

    public static DateTime FloorToHour(this DateTime m) => DateTime.MinValue + (m - DateTime.MinValue).FloorToHour();

    public static DateTime FloorToDay(this DateTime m) => DateTime.MinValue + (m - DateTime.MinValue).FloorToDay();

    public static DateTime FloorTo(this DateTime m, TimeSpan t) =>
        DateTime.MinValue + (m - DateTime.MinValue).FloorTo(t);

    public static DateTime RoundToSecond(this DateTime m) =>
        DateTime.MinValue + (m - DateTime.MinValue).RoundToSecond();

    public static DateTime RoundToMinute(this DateTime m) =>
        DateTime.MinValue + (m - DateTime.MinValue).RoundToMinute();

    public static DateTime RoundToHour(this DateTime m) => DateTime.MinValue + (m - DateTime.MinValue).RoundToHour();

    public static DateTime RoundToDay(this DateTime m) => DateTime.MinValue + (m - DateTime.MinValue).RoundToDay();

    public static DateTime RoundTo(this DateTime m, TimeSpan t) =>
        DateTime.MinValue + (m - DateTime.MinValue).RoundTo(t);

    public static DateTime CeilToSecond(this DateTime m) => DateTime.MinValue + (m - DateTime.MinValue).CeilToSecond();

    public static DateTime CeilToMinute(this DateTime m) => DateTime.MinValue + (m - DateTime.MinValue).CeilToMinute();

    public static DateTime CeilToHour(this DateTime m) => DateTime.MinValue + (m - DateTime.MinValue).CeilToHour();

    public static DateTime CeilToDay(this DateTime m) => DateTime.MinValue + (m - DateTime.MinValue).CeilToDay();

    public static DateTime InUtc(this DateTime m) => DateTime.SpecifyKind(m, DateTimeKind.Utc);

    public static long ToUnixTimeMinutes(this DateTime m) => (m.InUtc() - _unixEpoch).TotalMinutes.FloorInt64();

    public static long ToUnixTimeSeconds(this DateTime m) => (m.InUtc() - _unixEpoch).TotalSeconds.FloorInt64();

    public static long ToUnixTimeMilliseconds(this DateTime m) =>
        (m.InUtc() - _unixEpoch).TotalMilliseconds.FloorInt64();
}
