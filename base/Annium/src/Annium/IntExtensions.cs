using System;
using System.Runtime.CompilerServices;

namespace Annium;

/// <summary>
/// Provides extension methods for working with integers.
/// </summary>
public static class IntExtensions
{
    /// <summary>
    /// Ensures that a value is within a specified range.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="min">The minimum allowed value.</param>
    /// <param name="max">The maximum allowed value.</param>
    /// <returns>The value if it is within the range, or the nearest boundary value if it is outside the range.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Within(this int value, int min, int max) => value.Above(min).Below(max);

    /// <summary>
    /// Ensures that a value is not less than a specified minimum.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="min">The minimum allowed value.</param>
    /// <returns>The value if it is greater than or equal to the minimum, or the minimum value if it is less.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Above(this int value, int min) => Math.Max(value, min);

    /// <summary>
    /// Ensures that a value is not greater than a specified maximum.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="max">The maximum allowed value.</param>
    /// <returns>The value if it is less than or equal to the maximum, or the maximum value if it is greater.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Below(this int value, int max) => Math.Min(value, max);
}
