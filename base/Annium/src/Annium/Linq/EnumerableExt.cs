using System.Collections.Generic;

namespace Annium.Linq;

/// <summary>
/// Provides extension methods for working with enumerable sequences.
/// </summary>
public static class EnumerableExt
{
    /// <summary>
    /// Generates a sequence of integers within a specified range with a given step.
    /// </summary>
    /// <param name="start">The value of the first integer in the sequence.</param>
    /// <param name="count">The number of integers to generate.</param>
    /// <param name="step">The difference between consecutive integers in the sequence.</param>
    /// <returns>An enumerable sequence of integers.</returns>
    public static IEnumerable<int> Range(int start, int count, int step)
    {
        for (var i = 0; i < count; i++)
            yield return start + i * step;
    }

    /// <summary>
    /// Generates a sequence of decimal numbers within a specified range with a given step.
    /// </summary>
    /// <param name="start">The value of the first decimal number in the sequence.</param>
    /// <param name="count">The number of decimal numbers to generate.</param>
    /// <param name="step">The difference between consecutive decimal numbers in the sequence.</param>
    /// <returns>An enumerable sequence of decimal numbers.</returns>
    public static IEnumerable<decimal> Range(decimal start, int count, decimal step)
    {
        for (var i = 0; i < count; i++)
            yield return start + i * step;
    }
}
