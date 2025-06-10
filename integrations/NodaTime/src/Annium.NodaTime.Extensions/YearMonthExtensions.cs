using System;
using System.Runtime.CompilerServices;
using NodaTime;

namespace Annium.NodaTime.Extensions;

/// <summary>
/// Provides extension methods for working with NodaTime YearMonth objects, including navigation and arithmetic operations.
/// </summary>
public static class YearMonthExtensions
{
    /// <summary>
    /// Gets the YearMonth representing the same month in the next year.
    /// </summary>
    /// <param name="period">The YearMonth to advance.</param>
    /// <returns>A YearMonth representing the same month one year later.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static YearMonth NextYear(this YearMonth period) => period.AddYears(1);

    /// <summary>
    /// Gets the YearMonth representing the same month in the previous year.
    /// </summary>
    /// <param name="period">The YearMonth to move back.</param>
    /// <returns>A YearMonth representing the same month one year earlier.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static YearMonth PrevYear(this YearMonth period) => period.AddYears(-1);

    /// <summary>
    /// Gets the YearMonth representing the next month, automatically handling year transitions.
    /// </summary>
    /// <param name="period">The YearMonth to advance.</param>
    /// <returns>A YearMonth representing the next month.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static YearMonth NextMonth(this YearMonth period) => period.AddMonths(1);

    /// <summary>
    /// Gets the YearMonth representing the previous month, automatically handling year transitions.
    /// </summary>
    /// <param name="period">The YearMonth to move back.</param>
    /// <returns>A YearMonth representing the previous month.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static YearMonth PrevMonth(this YearMonth period) => period.AddMonths(-1);

    /// <summary>
    /// Adds the specified number of years to the YearMonth.
    /// </summary>
    /// <param name="period">The YearMonth to add years to.</param>
    /// <param name="value">The number of years to add (can be negative).</param>
    /// <returns>A new YearMonth with the specified number of years added.</returns>
    public static YearMonth AddYears(this YearMonth period, int value) =>
        new(period.Era, period.Year + value, period.Month);

    /// <summary>
    /// Adds the specified number of months to the YearMonth, automatically handling year rollovers.
    /// </summary>
    /// <param name="period">The YearMonth to add months to.</param>
    /// <param name="value">The number of months to add (can be negative).</param>
    /// <returns>A new YearMonth with the specified number of months added, with automatic year adjustments.</returns>
    public static YearMonth AddMonths(this YearMonth period, int value)
    {
        var totalMonths = period.Month + value;
        // totalMonths - 1 - hack to define YearMonth with 12 months'
        var addYears = (int)Math.Floor((decimal)(totalMonths - 1) / 12);
        var years = period.Year + addYears;
        var months = totalMonths - addYears * 12;

        return new YearMonth(period.Era, years, months);
    }
}
