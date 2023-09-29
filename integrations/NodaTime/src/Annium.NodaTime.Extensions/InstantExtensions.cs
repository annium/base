using System.Runtime.CompilerServices;
using NodaTime;

namespace Annium.NodaTime.Extensions;

public static class InstantExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Instant FromUnixTimeMinutes(long minutes) =>
        NodaConstants.UnixEpoch + Duration.FromMinutes(minutes);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsMidnight(this Instant m)
    {
        var d = m - Instant.MinValue;

        return d is { Hours: 0, Minutes: 0 } and { Seconds: 0, SubsecondTicks: 0 };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ToUnixTimeMinutes(this Instant m) =>
        m.Minus(NodaConstants.UnixEpoch).TotalMinutes.FloorInt64();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Instant FloorToSecond(this Instant m) =>
        Instant.MinValue + (m - Instant.MinValue).FloorToSecond();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Instant FloorToMinute(this Instant m) =>
        Instant.MinValue + (m - Instant.MinValue).FloorToMinute();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Instant FloorToHour(this Instant m) =>
        Instant.MinValue + (m - Instant.MinValue).FloorToHour();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Instant FloorToDay(this Instant m) =>
        Instant.MinValue + (m - Instant.MinValue).FloorToDay();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Instant FloorTo(this Instant m, Duration d) =>
        Instant.MinValue + (m - Instant.MinValue).FloorTo(d);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Instant RoundToSecond(this Instant m) =>
        Instant.MinValue + (m - Instant.MinValue).RoundToSecond();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Instant RoundToMinute(this Instant m) =>
        Instant.MinValue + (m - Instant.MinValue).RoundToMinute();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Instant RoundToHour(this Instant m) =>
        Instant.MinValue + (m - Instant.MinValue).RoundToHour();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Instant RoundToDay(this Instant m) =>
        Instant.MinValue + (m - Instant.MinValue).RoundToDay();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Instant RoundTo(this Instant m, Duration d) =>
        Instant.MinValue + (m - Instant.MinValue).RoundTo(d);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Instant CeilToSecond(this Instant m) =>
        Instant.MinValue + (m - Instant.MinValue).CeilToSecond();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Instant CeilToMinute(this Instant m) =>
        Instant.MinValue + (m - Instant.MinValue).CeilToMinute();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Instant CeilToHour(this Instant m) =>
        Instant.MinValue + (m - Instant.MinValue).CeilToHour();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Instant CeilToDay(this Instant m) =>
        Instant.MinValue + (m - Instant.MinValue).CeilToDay();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Instant CeilTo(this Instant m, Duration d) =>
        Instant.MinValue + (m - Instant.MinValue).CeilTo(d);
}