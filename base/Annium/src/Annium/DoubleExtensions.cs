using System;
using System.Runtime.CompilerServices;

namespace Annium;

/// <summary>
/// Provides extension methods for working with double-precision floating-point numbers.
/// </summary>
public static class DoubleExtensions
{
    /// <summary>
    /// Calculates the relative difference between two values.
    /// </summary>
    /// <param name="value">The value to compare.</param>
    /// <param name="from">The reference value.</param>
    /// <returns>The relative difference as a positive number. Returns infinity if the reference value is zero and the compared value is non-zero.</returns>
    public static double DiffFrom(this double value, double from) =>
        from == 0d
            ? value == 0d
                ? 0
                : double.PositiveInfinity
            : Math.Abs((value - from) / from);

    /// <summary>
    /// Determines whether a value is approximately equal to another value within a specified precision.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="to">The value to compare against.</param>
    /// <param name="precision">The maximum allowed relative difference.</param>
    /// <returns>true if the values are approximately equal; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAround(this double value, double to, double precision) => value.DiffFrom(to) <= precision;

    /// <summary>
    /// Rounds a value down to the nearest 32-bit integer.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <returns>The rounded value as a 32-bit integer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FloorInt32(this double value) => (int)Math.Floor(value);

    /// <summary>
    /// Rounds a value down to the nearest 64-bit integer.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <returns>The rounded value as a 64-bit integer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long FloorInt64(this double value) => (long)Math.Floor(value);

    /// <summary>
    /// Rounds a value down to the nearest double.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <returns>The rounded value as a double.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Floor(this double value) => Math.Floor(value);

    /// <summary>
    /// Rounds a value to the nearest 32-bit integer.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <returns>The rounded value as a 32-bit integer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int RoundInt32(this double value) => (int)Math.Round(value);

    /// <summary>
    /// Rounds a value to the nearest 32-bit integer using the specified rounding mode.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <param name="mode">The rounding mode to use.</param>
    /// <returns>The rounded value as a 32-bit integer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int RoundInt32(this double value, MidpointRounding mode) => (int)Math.Round(value, mode);

    /// <summary>
    /// Rounds a value to the nearest 64-bit integer.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <returns>The rounded value as a 64-bit integer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long RoundInt64(this double value) => (long)Math.Round(value);

    /// <summary>
    /// Rounds a value to the nearest 64-bit integer using the specified rounding mode.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <param name="mode">The rounding mode to use.</param>
    /// <returns>The rounded value as a 64-bit integer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long RoundInt64(this double value, MidpointRounding mode) => (long)Math.Round(value, mode);

    /// <summary>
    /// Rounds a value to the nearest double.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <returns>The rounded value as a double.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Round(this double value) => Math.Round(value);

    /// <summary>
    /// Rounds a value to a specified number of decimal places.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>The rounded value as a double.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Round(this double value, int digits) => Math.Round(value, digits);

    /// <summary>
    /// Rounds a value to the nearest double using the specified rounding mode.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <param name="mode">The rounding mode to use.</param>
    /// <returns>The rounded value as a double.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Round(this double value, MidpointRounding mode) => Math.Round(value, mode);

    /// <summary>
    /// Rounds a value to a specified number of decimal places using the specified rounding mode.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <param name="mode">The rounding mode to use.</param>
    /// <returns>The rounded value as a double.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Round(this double value, int digits, MidpointRounding mode) => Math.Round(value, digits, mode);

    /// <summary>
    /// Rounds a value up to the nearest 32-bit integer.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <returns>The rounded value as a 32-bit integer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CeilInt32(this double value) => (int)Math.Ceiling(value);

    /// <summary>
    /// Rounds a value up to the nearest 64-bit integer.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <returns>The rounded value as a 64-bit integer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long CeilInt64(this double value) => (long)Math.Ceiling(value);

    /// <summary>
    /// Rounds a value up to the nearest double.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <returns>The rounded value as a double.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Ceil(this double value) => Math.Ceiling(value);

    /// <summary>
    /// Ensures that a value is within a specified range.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="min">The minimum allowed value.</param>
    /// <param name="max">The maximum allowed value.</param>
    /// <returns>The value if it is within the range, or the nearest boundary value if it is outside the range.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Within(this double value, double min, double max) => value.Above(min).Below(max);

    /// <summary>
    /// Ensures that a value is not less than a specified minimum.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="min">The minimum allowed value.</param>
    /// <returns>The value if it is greater than or equal to the minimum, or the minimum value if it is less.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Above(this double value, double min) => Math.Max(value, min);

    /// <summary>
    /// Ensures that a value is not greater than a specified maximum.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="max">The maximum allowed value.</param>
    /// <returns>The value if it is less than or equal to the maximum, or the maximum value if it is greater.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Below(this double value, double max) => Math.Min(value, max);

    /// <summary>
    /// Rounds a value down to the nearest multiple of a specified step.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <param name="step">The step size to round to.</param>
    /// <returns>The rounded value as a double.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double FloorTo(this double value, double step) => value - value % step;

    /// <summary>
    /// Rounds a value to the nearest multiple of a specified step.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <param name="step">The step size to round to.</param>
    /// <returns>The rounded value as a double.</returns>
    public static double RoundTo(this double value, double step)
    {
        var diff = value % step;

        return value - diff + (step > diff * 2d ? 0d : step);
    }

    /// <summary>
    /// Rounds a value up to the nearest multiple of a specified step.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <param name="step">The step size to round to.</param>
    /// <returns>The rounded value as a double.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double CeilTo(this double value, double step) => value + step - value % step;
}
