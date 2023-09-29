using System.Runtime.CompilerServices;
using NodaTime;

namespace Annium.NodaTime.Extensions;

public static class LocalDateTimeExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalDateTime FromUnixTimeMinutes(long minutes) =>
        NodaConstants.UnixEpoch.Plus(Duration.FromMinutes(minutes)).InUtc().LocalDateTime;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalDateTime FromUnixTimeSeconds(long seconds) =>
        NodaConstants.UnixEpoch.Plus(Duration.FromSeconds(seconds)).InUtc().LocalDateTime;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalDateTime FromUnixTimeMilliseconds(long milliseconds) =>
        NodaConstants.UnixEpoch.Plus(Duration.FromMilliseconds(milliseconds)).InUtc().LocalDateTime;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static YearMonth GetYearMonth(this LocalDateTime m) =>
        new(m.Era, m.YearOfEra, m.Month, m.Calendar);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsMidnight(this LocalDateTime dateTime) =>
        dateTime is { Hour: 0, Minute: 0 } and { Second: 0, Millisecond: 0 };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ToUnixTimeMinutes(this LocalDateTime m) =>
        m.InUtc().ToInstant().Minus(NodaConstants.UnixEpoch).TotalMinutes.FloorInt64();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ToUnixTimeSeconds(this LocalDateTime m) =>
        m.InUtc().ToInstant().Minus(NodaConstants.UnixEpoch).TotalSeconds.FloorInt64();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ToUnixTimeMilliseconds(this LocalDateTime m) =>
        m.InUtc().ToInstant().Minus(NodaConstants.UnixEpoch).TotalMilliseconds.FloorInt64();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalDateTime FloorToSecond(this LocalDateTime m) =>
        m.FloorTo(Period.FromSeconds(1));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalDateTime FloorToMinute(this LocalDateTime m) =>
        m.FloorTo(Period.FromMinutes(1));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalDateTime FloorToHour(this LocalDateTime m) =>
        m.FloorTo(Period.FromHours(1));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalDateTime FloorToDay(this LocalDateTime m) =>
        m.FloorTo(Period.FromDays(1));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalDateTime FloorTo(this LocalDateTime m, Period p) =>
        m.InUtc().ToInstant().FloorTo(p.ToDuration()).InUtc().LocalDateTime;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalDateTime RoundToSecond(this LocalDateTime m) =>
        m.RoundTo(Period.FromSeconds(1));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalDateTime RoundToMinute(this LocalDateTime m) =>
        m.RoundTo(Period.FromMinutes(1));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalDateTime RoundToHour(this LocalDateTime m) =>
        m.RoundTo(Period.FromHours(1));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalDateTime RoundToDay(this LocalDateTime m) =>
        m.RoundTo(Period.FromDays(1));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalDateTime RoundTo(this LocalDateTime m, Period p) =>
        m.InUtc().ToInstant().RoundTo(p.ToDuration()).InUtc().LocalDateTime;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalDateTime CeilToSecond(this LocalDateTime m) =>
        m.CeilTo(Period.FromSeconds(1));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalDateTime CeilToMinute(this LocalDateTime m) =>
        m.CeilTo(Period.FromMinutes(1));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalDateTime CeilToHour(this LocalDateTime m) =>
        m.CeilTo(Period.FromHours(1));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalDateTime CeilToDay(this LocalDateTime m) =>
        m.CeilTo(Period.FromDays(1));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalDateTime CeilTo(this LocalDateTime m, Period p) =>
        m.InUtc().ToInstant().CeilTo(p.ToDuration()).InUtc().LocalDateTime;
}