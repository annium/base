using System.Runtime.CompilerServices;
using NodaTime;

namespace Annium.NodaTime.Extensions;

/// <summary>
/// Provides extension methods for working with NodaTime Instant objects, including Unix time conversions, timezone operations, and temporal rounding.
/// </summary>
public static class InstantExtensions
{
    /// <summary>
    /// The system's default time zone for local time conversions.
    /// </summary>
    private static readonly DateTimeZone _localTz = DateTimeZoneProviders.Tzdb.GetSystemDefault();

    /// <summary>
    /// Creates an Instant from Unix time in minutes since the Unix epoch.
    /// </summary>
    /// <param name="minutes">The number of minutes since the Unix epoch (1970-01-01 00:00:00 UTC).</param>
    /// <returns>An Instant representing the specified Unix time.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Instant FromUnixTimeMinutes(long minutes) => NodaConstants.UnixEpoch + Duration.FromMinutes(minutes);

    /// <summary>
    /// Determines whether the instant represents midnight (00:00:00.000).
    /// </summary>
    /// <param name="m">The instant to check.</param>
    /// <returns>True if the instant represents midnight; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsMidnight(this Instant m)
    {
        var d = m - Instant.MinValue;

        return d is { Hours: 0, Minutes: 0 } and { Seconds: 0, SubsecondTicks: 0 };
    }

    /// <summary>
    /// Converts the instant to a ZonedDateTime in the system's local time zone.
    /// </summary>
    /// <param name="m">The instant to convert.</param>
    /// <returns>A ZonedDateTime representing the instant in the local time zone.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ZonedDateTime InLocal(this Instant m) => m.InZone(_localTz);

    /// <summary>
    /// Converts the instant to Unix time in minutes since the Unix epoch.
    /// </summary>
    /// <param name="m">The instant to convert.</param>
    /// <returns>The number of minutes since the Unix epoch (1970-01-01 00:00:00 UTC).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ToUnixTimeMinutes(this Instant m) => m.Minus(NodaConstants.UnixEpoch).TotalMinutes.FloorInt64();

    /// <summary>
    /// Floors the instant to the nearest second by truncating any subsecond components.
    /// </summary>
    /// <param name="m">The instant to floor.</param>
    /// <returns>A new instant with subsecond components removed.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Instant FloorToSecond(this Instant m) => Instant.MinValue + (m - Instant.MinValue).FloorToSecond();

    /// <summary>
    /// Floors the instant to the nearest minute by truncating any subsecond and second components.
    /// </summary>
    /// <param name="m">The instant to floor.</param>
    /// <returns>A new instant with components smaller than a minute removed.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Instant FloorToMinute(this Instant m) => Instant.MinValue + (m - Instant.MinValue).FloorToMinute();

    /// <summary>
    /// Floors the instant to the nearest hour by truncating any components smaller than an hour.
    /// </summary>
    /// <param name="m">The instant to floor.</param>
    /// <returns>A new instant with components smaller than an hour removed.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Instant FloorToHour(this Instant m) => Instant.MinValue + (m - Instant.MinValue).FloorToHour();

    /// <summary>
    /// Floors the instant to the nearest day by truncating any components smaller than a day.
    /// </summary>
    /// <param name="m">The instant to floor.</param>
    /// <returns>A new instant with components smaller than a day removed.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Instant FloorToDay(this Instant m) => Instant.MinValue + (m - Instant.MinValue).FloorToDay();

    /// <summary>
    /// Floors the instant to the nearest multiple of the specified duration by truncating any remainder.
    /// </summary>
    /// <param name="m">The instant to floor.</param>
    /// <param name="d">The duration unit to floor to.</param>
    /// <returns>A new instant that is the largest multiple of the unit duration that is less than or equal to the input instant.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Instant FloorTo(this Instant m, Duration d) => Instant.MinValue + (m - Instant.MinValue).FloorTo(d);

    /// <summary>
    /// Rounds the instant to the nearest second using standard rounding rules.
    /// </summary>
    /// <param name="m">The instant to round.</param>
    /// <returns>A new instant rounded to the nearest second.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Instant RoundToSecond(this Instant m) => Instant.MinValue + (m - Instant.MinValue).RoundToSecond();

    /// <summary>
    /// Rounds the instant to the nearest minute using standard rounding rules.
    /// </summary>
    /// <param name="m">The instant to round.</param>
    /// <returns>A new instant rounded to the nearest minute.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Instant RoundToMinute(this Instant m) => Instant.MinValue + (m - Instant.MinValue).RoundToMinute();

    /// <summary>
    /// Rounds the instant to the nearest hour using standard rounding rules.
    /// </summary>
    /// <param name="m">The instant to round.</param>
    /// <returns>A new instant rounded to the nearest hour.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Instant RoundToHour(this Instant m) => Instant.MinValue + (m - Instant.MinValue).RoundToHour();

    /// <summary>
    /// Rounds the instant to the nearest day using standard rounding rules.
    /// </summary>
    /// <param name="m">The instant to round.</param>
    /// <returns>A new instant rounded to the nearest day.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Instant RoundToDay(this Instant m) => Instant.MinValue + (m - Instant.MinValue).RoundToDay();

    /// <summary>
    /// Rounds the instant to the nearest multiple of the specified duration using standard rounding rules.
    /// </summary>
    /// <param name="m">The instant to round.</param>
    /// <param name="d">The duration unit to round to.</param>
    /// <returns>A new instant that is the closest multiple of the unit duration to the input instant.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Instant RoundTo(this Instant m, Duration d) => Instant.MinValue + (m - Instant.MinValue).RoundTo(d);

    /// <summary>
    /// Ceils the instant to the next second by rounding up any subsecond components.
    /// </summary>
    /// <param name="m">The instant to ceil.</param>
    /// <returns>A new instant rounded up to the next second.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Instant CeilToSecond(this Instant m) => Instant.MinValue + (m - Instant.MinValue).CeilToSecond();

    /// <summary>
    /// Ceils the instant to the next minute by rounding up any components smaller than a minute.
    /// </summary>
    /// <param name="m">The instant to ceil.</param>
    /// <returns>A new instant rounded up to the next minute.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Instant CeilToMinute(this Instant m) => Instant.MinValue + (m - Instant.MinValue).CeilToMinute();

    /// <summary>
    /// Ceils the instant to the next hour by rounding up any components smaller than an hour.
    /// </summary>
    /// <param name="m">The instant to ceil.</param>
    /// <returns>A new instant rounded up to the next hour.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Instant CeilToHour(this Instant m) => Instant.MinValue + (m - Instant.MinValue).CeilToHour();

    /// <summary>
    /// Ceils the instant to the next day by rounding up any components smaller than a day.
    /// </summary>
    /// <param name="m">The instant to ceil.</param>
    /// <returns>A new instant rounded up to the next day.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Instant CeilToDay(this Instant m) => Instant.MinValue + (m - Instant.MinValue).CeilToDay();

    /// <summary>
    /// Ceils the instant to the next multiple of the specified duration by rounding up any remainder.
    /// </summary>
    /// <param name="m">The instant to ceil.</param>
    /// <param name="d">The duration unit to ceil to.</param>
    /// <returns>A new instant that is the smallest multiple of the unit duration that is greater than or equal to the input instant.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Instant CeilTo(this Instant m, Duration d) => Instant.MinValue + (m - Instant.MinValue).CeilTo(d);
}
