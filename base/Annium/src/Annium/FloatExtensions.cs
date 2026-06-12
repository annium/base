using System;
using System.Runtime.CompilerServices;

namespace Annium;

/// <summary>
/// Provides extension methods for working with single-precision floating-point numbers.
/// </summary>
public static class FloatExtensions
{
    /// <summary>
    /// Error message thrown by step-validating methods (FloorTo, CeilTo) when step is non-finite or non-positive.
    /// </summary>
    private const string StepError = "Step must be a positive, finite number.";

    /// <summary>
    /// Calculates the relative difference between two float values.
    /// </summary>
    /// <param name="value">The value to compare.</param>
    /// <param name="from">The reference value.</param>
    /// <returns>The relative difference as a float value. Returns 0 if both values are 0, or positive infinity if the reference value is 0 and the compared value is not.</returns>
    public static float DiffFrom(this float value, float from) =>
        from == 0f
            ? value == 0f
                ? 0
                : float.PositiveInfinity
            : Math.Abs((value - from) / from);

    /// <summary>
    /// Determines if a float value is approximately equal to another value within a specified precision.
    /// </summary>
    /// <param name="value">The value to compare.</param>
    /// <param name="to">The reference value.</param>
    /// <param name="precision">The maximum allowed difference between the values.</param>
    /// <returns>true if the values are approximately equal within the specified precision; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAround(this float value, float to, float precision) => value.DiffFrom(to) <= precision;

    /// <summary>
    /// Rounds a float value down to the nearest integer.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <returns>The rounded value as a 32-bit integer.</returns>
    /// <exception cref="OverflowException">Thrown when <paramref name="value"/> is NaN or outside the Int32 range.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FloorInt32(this float value) => checked((int)Math.Floor(value));

    /// <summary>
    /// Rounds a float value down to the nearest integer.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <returns>The rounded value as a 64-bit integer.</returns>
    /// <exception cref="OverflowException">Thrown when <paramref name="value"/> is NaN or outside the Int64 range.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long FloorInt64(this float value) => checked((long)Math.Floor(value));

    /// <summary>
    /// Rounds a float value down to the nearest float.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <returns>The rounded value as a float.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Floor(this float value) => (float)Math.Floor(value);

    /// <summary>
    /// Rounds a float value to the nearest integer.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <returns>The rounded value as a 32-bit integer.</returns>
    /// <exception cref="OverflowException">Thrown when <paramref name="value"/> is NaN or outside the Int32 range.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int RoundInt32(this float value) => checked((int)Math.Round(value));

    /// <summary>
    /// Rounds a float value to the nearest integer using the specified rounding mode.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <param name="mode">The rounding mode to use.</param>
    /// <returns>The rounded value as a 32-bit integer.</returns>
    /// <exception cref="OverflowException">Thrown when <paramref name="value"/> is NaN or outside the Int32 range.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int RoundInt32(this float value, MidpointRounding mode) => checked((int)Math.Round(value, mode));

    /// <summary>
    /// Rounds a float value to the nearest integer.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <returns>The rounded value as a 64-bit integer.</returns>
    /// <exception cref="OverflowException">Thrown when <paramref name="value"/> is NaN or outside the Int64 range.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long RoundInt64(this float value) => checked((long)Math.Round(value));

    /// <summary>
    /// Rounds a float value to the nearest integer using the specified rounding mode.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <param name="mode">The rounding mode to use.</param>
    /// <returns>The rounded value as a 64-bit integer.</returns>
    /// <exception cref="OverflowException">Thrown when <paramref name="value"/> is NaN or outside the Int64 range.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long RoundInt64(this float value, MidpointRounding mode) => checked((long)Math.Round(value, mode));

    /// <summary>
    /// Rounds a float value to the nearest float.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <returns>The rounded value as a float.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Round(this float value) => (float)Math.Round(value);

    /// <summary>
    /// Rounds a float value to a specified number of decimal places.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <returns>The rounded value as a float.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Round(this float value, int digits) => (float)Math.Round(value, digits);

    /// <summary>
    /// Rounds a float value to the nearest float using the specified rounding mode.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <param name="mode">The rounding mode to use.</param>
    /// <returns>The rounded value as a float.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Round(this float value, MidpointRounding mode) => (float)Math.Round(value, mode);

    /// <summary>
    /// Rounds a float value to a specified number of decimal places using the specified rounding mode.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <param name="digits">The number of decimal places to round to.</param>
    /// <param name="mode">The rounding mode to use.</param>
    /// <returns>The rounded value as a float.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Round(this float value, int digits, MidpointRounding mode) =>
        (float)Math.Round(value, digits, mode);

    /// <summary>
    /// Rounds a float value up to the nearest integer.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <returns>The rounded value as a 32-bit integer.</returns>
    /// <exception cref="OverflowException">Thrown when <paramref name="value"/> is NaN or outside the Int32 range.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CeilInt32(this float value) => checked((int)Math.Ceiling(value));

    /// <summary>
    /// Rounds a float value up to the nearest integer.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <returns>The rounded value as a 64-bit integer.</returns>
    /// <exception cref="OverflowException">Thrown when <paramref name="value"/> is NaN or outside the Int64 range.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long CeilInt64(this float value) => checked((long)Math.Ceiling(value));

    /// <summary>
    /// Rounds a float value up to the nearest float.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <returns>The rounded value as a float.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Ceil(this float value) => (float)Math.Ceiling(value);

    /// <summary>
    /// Rounds a float value down to the nearest multiple of a specified step.
    /// </summary>
    /// <param name="value">The value to round. <see cref="float.NaN"/> and infinities propagate to the result.</param>
    /// <param name="step">The step value to round to. Must be a positive, finite number.</param>
    /// <returns>The rounded value as a float.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="step"/> is non-finite or not positive.</exception>
    public static float FloorTo(this float value, float step)
    {
        if (!float.IsFinite(step) || step <= 0f)
            throw new ArgumentOutOfRangeException(nameof(step), step, StepError);

        var rem = ((value % step) + step) % step;
        return value - rem;
    }

    /// <summary>
    /// Rounds a float value to the nearest multiple of a specified step.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <param name="step">The step value to round to.</param>
    /// <returns>The rounded value as a float.</returns>
    public static float RoundTo(this float value, float step)
    {
        var rem = ((value % step) + step) % step;

        return rem * 2f < step ? value - rem : value - rem + step;
    }

    /// <summary>
    /// Rounds a float value up to the nearest multiple of a specified step.
    /// </summary>
    /// <param name="value">The value to round. <see cref="float.NaN"/> and infinities propagate to the result.</param>
    /// <param name="step">The step value to round to. Must be a positive, finite number.</param>
    /// <returns>The rounded value as a float.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="step"/> is non-finite or not positive.</exception>
    public static float CeilTo(this float value, float step)
    {
        if (!float.IsFinite(step) || step <= 0f)
            throw new ArgumentOutOfRangeException(nameof(step), step, StepError);

        var rem = ((value % step) + step) % step;
        return rem == 0f ? value : value - rem + step;
    }
}
