using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Annium;

/// <summary>
/// Provides extension methods for working with decimal numbers.
/// </summary>
public static class DecimalExtensions
{
    /// <summary>
    /// Calculates the relative difference between two decimal values.
    /// </summary>
    /// <param name="value">The value to compare.</param>
    /// <param name="from">The reference value.</param>
    /// <returns>The relative difference as a decimal value. Returns 0 if both values are 0, or decimal.MaxValue if the reference value is 0 and the compared value is not.</returns>
    public static decimal DiffFrom(this decimal value, decimal from) =>
        from == 0m
            ? value == 0m
                ? 0
                : decimal.MaxValue
            : value.DiffFromInternal(from);

    /// <summary>
    /// Determines if a decimal value is approximately equal to another value within a specified precision.
    /// </summary>
    /// <param name="value">The value to compare.</param>
    /// <param name="to">The reference value.</param>
    /// <param name="precision">The maximum allowed difference between the values.</param>
    /// <returns>true if the values are approximately equal within the specified precision; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAround(this decimal value, decimal to, decimal precision) => value.DiffFrom(to) <= precision;

    /// <summary>
    /// Rounds a decimal value down to the nearest integer.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <returns>The rounded value as a 32-bit integer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FloorInt32(this decimal value) => (int)Math.Floor(value);

    /// <summary>
    /// Rounds a decimal value down to the nearest integer.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <returns>The rounded value as a 64-bit integer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long FloorInt64(this decimal value) => (long)Math.Floor(value);

    /// <summary>
    /// Rounds a decimal value down to the nearest decimal.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <returns>The rounded value as a decimal.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static decimal Floor(this decimal value) => Math.Floor(value);

    /// <summary>
    /// Rounds a decimal value to the nearest integer.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <returns>The rounded value as a 32-bit integer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int RoundInt32(this decimal value) => (int)Math.Round(value);

    /// <summary>
    /// Rounds a decimal value to the nearest integer using the specified rounding mode.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <param name="mode">The rounding mode to use.</param>
    /// <returns>The rounded value as a 32-bit integer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int RoundInt32(this decimal value, MidpointRounding mode) => (int)Math.Round(value, mode);

    /// <summary>
    /// Rounds a decimal value to the nearest integer.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <returns>The rounded value as a 64-bit integer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long RoundInt64(this decimal value) => (long)Math.Round(value);

    /// <summary>
    /// Rounds a decimal value to the nearest integer using the specified rounding mode.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <param name="mode">The rounding mode to use.</param>
    /// <returns>The rounded value as a 64-bit integer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long RoundInt64(this decimal value, MidpointRounding mode) => (long)Math.Round(value, mode);

    /// <summary>
    /// Rounds a decimal value to the nearest decimal.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <returns>The rounded value as a decimal.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static decimal Round(this decimal value) => Math.Round(value);

    /// <summary>
    /// Rounds a decimal value to a specified number of decimal places.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>The rounded value as a decimal.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static decimal Round(this decimal value, int digits) => Math.Round(value, digits);

    /// <summary>
    /// Rounds a decimal value to the nearest decimal using the specified rounding mode.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <param name="mode">The rounding mode to use.</param>
    /// <returns>The rounded value as a decimal.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static decimal Round(this decimal value, MidpointRounding mode) => Math.Round(value, mode);

    /// <summary>
    /// Rounds a decimal value to a specified number of decimal places using the specified rounding mode.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <param name="mode">The rounding mode to use.</param>
    /// <returns>The rounded value as a decimal.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static decimal Round(this decimal value, int digits, MidpointRounding mode) =>
        Math.Round(value, digits, mode);

    /// <summary>
    /// Rounds a decimal value up to the nearest integer.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <returns>The rounded value as a 32-bit integer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CeilInt32(this decimal value) => (int)Math.Ceiling(value);

    /// <summary>
    /// Rounds a decimal value up to the nearest integer.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <returns>The rounded value as a 64-bit integer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long CeilInt64(this decimal value) => (long)Math.Ceiling(value);

    /// <summary>
    /// Rounds a double value up to the nearest double.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <returns>The rounded value as a double.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Ceil(this double value) => Math.Ceiling(value);

    /// <summary>
    /// Ensures a decimal value is within a specified range.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="min">The minimum allowed value.</param>
    /// <param name="max">The maximum allowed value.</param>
    /// <returns>The value if it is within the range, or the nearest boundary value if it is outside the range.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static decimal Within(this decimal value, decimal min, decimal max) => value.Above(min).Below(max);

    /// <summary>
    /// Ensures a decimal value is not less than a specified minimum.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="min">The minimum allowed value.</param>
    /// <returns>The value if it is greater than or equal to the minimum, or the minimum value if it is less.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static decimal Above(this decimal value, decimal min) => Math.Max(value, min);

    /// <summary>
    /// Ensures a decimal value is not greater than a specified maximum.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="max">The maximum allowed value.</param>
    /// <returns>The value if it is less than or equal to the maximum, or the maximum value if it is greater.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static decimal Below(this decimal value, decimal max) => Math.Min(value, max);

    /// <summary>
    /// Formats a decimal value to a "pretty" representation by removing unnecessary decimal places while maintaining a specified precision.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <param name="diff">The maximum allowed difference between the original and formatted values.</param>
    /// <returns>A decimal value with unnecessary decimal places removed while maintaining the specified precision.</returns>
    public static decimal ToPretty(this decimal value, decimal diff)
    {
        if (value == 0m)
            return 0;

        var result = value;

        var decimals = value.Decimals();
        if (decimals > 0)
        {
            while (decimals > 0)
            {
                var next = Math.Round(result, --decimals);
                if (next.DiffFromInternal(value) > diff)
                    return result;

                result = next;
            }
        }

        var mul = 10;
        while (true)
        {
            var next = Math.Round(result / mul) * mul;
            if (next.DiffFromInternal(value) > diff)
                return result;

            result = next;
            mul *= 10;
        }
    }

    /// <summary>
    /// Rounds a decimal value down to the nearest multiple of a specified step.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <param name="step">The step value to round to.</param>
    /// <returns>The rounded value as a decimal.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static decimal FloorTo(this decimal value, decimal step) => value - value % step;

    /// <summary>
    /// Rounds a decimal value to the nearest multiple of a specified step.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <param name="step">The step value to round to.</param>
    /// <returns>The rounded value as a decimal.</returns>
    public static decimal RoundTo(this decimal value, decimal step)
    {
        var diff = value % step;

        return value - diff + (step > diff * 2m ? 0m : step);
    }

    /// <summary>
    /// Rounds a decimal value up to the nearest multiple of a specified step.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <param name="step">The step value to round to.</param>
    /// <returns>The rounded value as a decimal.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static decimal CeilTo(this decimal value, decimal step) => value + step - value % step;

    /// <summary>
    /// Aligns a decimal value by removing trailing zeros.
    /// </summary>
    /// <param name="value">The value to align.</param>
    /// <returns>The aligned value as a decimal.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static decimal Align(this decimal value)
    {
        while (true)
        {
            var bits = decimal.GetBits(value);
            var scale = (byte)((bits[3] >> 16) & 0x7F);

            if (bits[0] % 10 != 0 || scale == 0)
                break;

            var sign = (bits[3] & 0x80000000) != 0;

            value = new decimal(bits[0] / 10, bits[1], bits[2], sign, (byte)(scale - 1));
        }

        return value;
    }

    /// <summary>
    /// Gets the number of decimal places in a decimal value.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>The number of decimal places.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Decimals(this decimal value) => decimal.GetBits(value)[3] >> 16;

    /// <summary>
    /// Normalizes a decimal value by removing any trailing zeros.
    /// </summary>
    /// <param name="value">The value to normalize.</param>
    /// <returns>The normalized value as a decimal.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static decimal Normalize(this decimal value) => value / 1.000000000000000M;

    /// <summary>
    /// Converts a decimal value to its string representation using the general format and invariant culture.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The string representation of the value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToGeneralInvariantString(this decimal value) =>
        value.ToString("G", CultureInfo.InvariantCulture);

    /// <summary>
    /// Calculates the relative difference between two decimal values.
    /// </summary>
    /// <param name="value">The value to compare.</param>
    /// <param name="from">The reference value.</param>
    /// <returns>The relative difference as a decimal value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static decimal DiffFromInternal(this decimal value, decimal from) => Math.Abs((value - from) / from);
}
