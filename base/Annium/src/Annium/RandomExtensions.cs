using System;
using System.Linq;

namespace Annium;

/// <summary>
/// Provides extension methods for working with random number generation.
/// </summary>
public static class RandomExtensions
{
    /// <summary>
    /// Returns a random boolean value.
    /// </summary>
    /// <param name="random">The random number generator.</param>
    /// <returns>A random boolean value with equal probability of true or false.</returns>
    public static bool NextBool(this Random random) => random.Next(0, 1) == 1;

    /// <summary>
    /// Returns a random enumeration value.
    /// </summary>
    /// <typeparam name="T">The enumeration type.</typeparam>
    /// <param name="random">The random number generator.</param>
    /// <param name="values">Optional array of values to choose from. If not provided, all values of the enumeration will be used.</param>
    /// <returns>A random value from the specified enumeration.</returns>
    /// <exception cref="ArgumentException">Thrown when the values array is empty and the enumeration has no values.</exception>
    public static T NextEnum<T>(this Random random, params T[] values)
        where T : Enum
    {
        if (values.Length == 0)
            values = Enum.GetValues(typeof(T)).OfType<T>().ToArray();

        return values[random.Next(0, values.Length)];
    }

    /// <summary>
    /// Returns a random decimal value between 0 and 1.
    /// </summary>
    /// <param name="random">The random number generator.</param>
    /// <returns>A random decimal value between 0 and 1.</returns>
    public static decimal NextDecimal(this Random random) => (decimal)random.NextDouble();
}
