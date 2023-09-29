using System.Runtime.CompilerServices;
using NodaTime;

namespace Annium.NodaTime.Extensions;

public static class ZonedDateTimeExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ZonedDateTime FromUnixTimeMinutes(long minutes) =>
        NodaConstants.UnixEpoch.Plus(Duration.FromMinutes(minutes)).InUtc();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ZonedDateTime FromUnixTimeSeconds(long seconds) =>
        NodaConstants.UnixEpoch.Plus(Duration.FromSeconds(seconds)).InUtc();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ZonedDateTime FromUnixTimeMilliseconds(long milliseconds) =>
        NodaConstants.UnixEpoch.Plus(Duration.FromMilliseconds(milliseconds)).InUtc();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static YearMonth GetYearMonth(this ZonedDateTime m) =>
        new(m.Era, m.YearOfEra, m.Month, m.Calendar);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ToUnixTimeMinutes(this ZonedDateTime m) =>
        m.ToInstant().Minus(NodaConstants.UnixEpoch).TotalMinutes.FloorInt64();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ToUnixTimeSeconds(this ZonedDateTime m) =>
        m.ToInstant().Minus(NodaConstants.UnixEpoch).TotalSeconds.FloorInt64();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ToUnixTimeMilliseconds(this ZonedDateTime m) =>
        m.ToInstant().Minus(NodaConstants.UnixEpoch).TotalMilliseconds.FloorInt64();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ZonedDateTime FloorToSecond(this ZonedDateTime m) =>
        m.FloorTo(Period.FromSeconds(1));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ZonedDateTime FloorToMinute(this ZonedDateTime m) =>
        m.FloorTo(Period.FromMinutes(1));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ZonedDateTime FloorToHour(this ZonedDateTime m) =>
        m.FloorTo(Period.FromHours(1));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ZonedDateTime FloorToDay(this ZonedDateTime m) =>
        m.FloorTo(Period.FromDays(1));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ZonedDateTime FloorTo(this ZonedDateTime m, Period p) =>
        new(m.ToInstant().FloorTo(p.ToDuration()), m.Zone, m.Calendar);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ZonedDateTime RoundToSecond(this ZonedDateTime m) =>
        m.RoundTo(Period.FromSeconds(1));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ZonedDateTime RoundToMinute(this ZonedDateTime m) =>
        m.RoundTo(Period.FromMinutes(1));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ZonedDateTime RoundToHour(this ZonedDateTime m) =>
        m.RoundTo(Period.FromHours(1));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ZonedDateTime RoundToDay(this ZonedDateTime m) =>
        m.RoundTo(Period.FromDays(1));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ZonedDateTime RoundTo(this ZonedDateTime m, Period p) =>
        new(m.ToInstant().RoundTo(p.ToDuration()), m.Zone, m.Calendar);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ZonedDateTime CeilToSecond(this ZonedDateTime m) =>
        m.CeilTo(Period.FromSeconds(1));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ZonedDateTime CeilToMinute(this ZonedDateTime m) =>
        m.CeilTo(Period.FromMinutes(1));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ZonedDateTime CeilToHour(this ZonedDateTime m) =>
        m.CeilTo(Period.FromHours(1));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ZonedDateTime CeilToDay(this ZonedDateTime m) =>
        m.CeilTo(Period.FromDays(1));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ZonedDateTime CeilTo(this ZonedDateTime m, Period p) =>
        new(m.ToInstant().CeilTo(p.ToDuration()), m.Zone, m.Calendar);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsMidnight(this ZonedDateTime dateTime) =>
        dateTime is { Hour: 0, Minute: 0 } and { Second: 0, Millisecond: 0 };
}