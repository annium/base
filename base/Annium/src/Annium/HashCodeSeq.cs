using System;
using System.Collections.Generic;

namespace Annium;

/// <summary>
/// Provides methods for computing hash codes for sequences of values.
/// </summary>
public static class HashCodeSeq
{
    /// <summary>
    /// Combines the hash codes of a sequence of key-value pairs.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the sequence.</typeparam>
    /// <typeparam name="TValue">The type of the values in the sequence.</typeparam>
    /// <param name="seq">The sequence of key-value pairs to compute the hash code for.</param>
    /// <returns>A hash code that represents the sequence of key-value pairs.</returns>
    public static int Combine<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> seq)
        where TKey : notnull
        where TValue : notnull
    {
        unchecked
        {
            var hash = 19;

            foreach (var x in seq)
                hash = hash * 31 + HashCode.Combine(x.Key.GetHashCode(), x.Value.GetHashCode());

            return hash;
        }
    }

    /// <summary>
    /// Combines the hash codes of a sequence of values.
    /// </summary>
    /// <typeparam name="T">The type of the values in the sequence.</typeparam>
    /// <param name="seq">The sequence of values to compute the hash code for.</param>
    /// <returns>A hash code that represents the sequence of values.</returns>
    public static int Combine<T>(IEnumerable<T> seq)
        where T : notnull
    {
        unchecked
        {
            var hash = 19;
            foreach (var x in seq)
                hash = hash * 31 + x.GetHashCode();

            return hash;
        }
    }
}
