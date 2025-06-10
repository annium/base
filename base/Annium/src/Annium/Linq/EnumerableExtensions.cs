using System;
using System.Collections.Generic;
using System.Linq;

namespace Annium.Linq;

/// <summary>
/// Provides extension methods for working with enumerable collections.
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// Random number generator used for shuffling collections.
    /// </summary>
    private static readonly Random _random = new();

    /// <summary>
    /// Concatenates the elements of a string collection, using the specified separator between each element.
    /// </summary>
    /// <param name="src">The collection of strings to concatenate.</param>
    /// <param name="separator">The string to use as a separator. Default is an empty string.</param>
    /// <returns>A string that consists of the elements of the collection delimited by the separator string.</returns>
    public static string Join(this IEnumerable<string> src, string separator = "") => string.Join(separator, src);

    /// <summary>
    /// Returns a sequence containing a single element if the source is not null.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="src">The source element.</param>
    /// <returns>A sequence containing the source element if it's not null, otherwise an empty sequence.</returns>
    public static IEnumerable<T> Yield<T>(this T src)
    {
        if (src is null)
            yield break;

        yield return src;
    }

    /// <summary>
    /// Randomly shuffles the elements of a sequence.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="src">The source sequence.</param>
    /// <returns>A new sequence with the elements randomly reordered.</returns>
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> src)
    {
        return src.OrderBy(_ => _random.Next(0, 1) == 1);
    }

    /// <summary>
    /// Filters a sequence of values based on a predicate, returning elements that do not match the predicate.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="src">The source sequence.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <returns>A sequence containing elements from the source sequence that do not satisfy the predicate.</returns>
    public static IEnumerable<T> WhereNot<T>(this IEnumerable<T> src, Func<T, bool> predicate)
    {
        foreach (var element in src)
            if (!predicate(element))
                yield return element;
    }

    /// <summary>
    /// Determines whether no element of a sequence satisfies a condition.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="src">The source sequence.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <returns>true if no elements in the source sequence pass the test in the specified predicate; otherwise, false.</returns>
    public static bool None<T>(this IEnumerable<T> src, Func<T, bool> predicate)
    {
        foreach (var element in src)
            if (predicate(element))
                return false;

        return true;
    }

    /// <summary>
    /// Creates a sorted list from a sequence of elements using a key selector function.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="items">The source sequence.</param>
    /// <param name="getKey">A function to extract a key from each element.</param>
    /// <returns>A sorted list containing the elements from the source sequence, sorted by their keys.</returns>
    public static SortedList<TKey, T> ToSortedList<TKey, T>(this IEnumerable<T> items, Func<T, TKey> getKey)
        where TKey : notnull
    {
        return new SortedList<TKey, T>(items.ToDictionary(getKey, x => x));
    }

    /// <summary>
    /// Computes the Cartesian product of a sequence of sequences.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="sequences">The sequence of sequences to compute the Cartesian product of.</param>
    /// <returns>A sequence of sequences representing the Cartesian product of the input sequences.</returns>
    public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
    {
        IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>() };

        return sequences.Aggregate(
            emptyProduct,
            (accumulator, sequence) => from acc in accumulator from item in sequence select acc.Concat(new[] { item })
        );
    }
}
