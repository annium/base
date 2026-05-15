using System.Numerics;
using System.Runtime.CompilerServices;

namespace Annium;

/// <summary>
/// Provides numeric clamping extensions for any <see cref="INumber{T}"/> type.
/// </summary>
public static class NumericExtensions
{
    /// <summary>
    /// Ensures that a value is within a specified range.
    /// </summary>
    /// <typeparam name="T">The numeric type.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="min">The minimum allowed value.</param>
    /// <param name="max">The maximum allowed value.</param>
    /// <returns>The value if it is within the range, or the nearest boundary value if it is outside the range.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Within<T>(this T value, T min, T max)
        where T : INumber<T> => value.Above(min).Below(max);

    /// <summary>
    /// Ensures that a value is not less than a specified minimum.
    /// </summary>
    /// <typeparam name="T">The numeric type.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="min">The minimum allowed value.</param>
    /// <returns>The value if it is greater than or equal to the minimum, or the minimum value if it is less.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Above<T>(this T value, T min)
        where T : INumber<T> => T.Max(value, min);

    /// <summary>
    /// Ensures that a value is not greater than a specified maximum.
    /// </summary>
    /// <typeparam name="T">The numeric type.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="max">The maximum allowed value.</param>
    /// <returns>The value if it is less than or equal to the maximum, or the maximum value if it is greater.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Below<T>(this T value, T max)
        where T : INumber<T> => T.Min(value, max);
}
