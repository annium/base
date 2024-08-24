using System;
using System.Collections.Generic;
using System.Linq;

namespace Annium.Linq;

public static class EnumerableExtensions
{
    private static readonly Random _random = new();

    public static string Join(this IEnumerable<string> src, string separator = "") => string.Join(separator, src);

    public static IEnumerable<T> Yield<T>(this T src)
    {
        if (src is null)
            yield break;

        yield return src;
    }

    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> src)
    {
        return src.OrderBy(_ => _random.Next(0, 1) == 1);
    }

    public static IEnumerable<T> WhereNot<T>(this IEnumerable<T> src, Func<T, bool> predicate)
    {
        foreach (var element in src)
            if (!predicate(element))
                yield return element;
    }

    public static bool None<T>(this IEnumerable<T> src, Func<T, bool> predicate)
    {
        foreach (var element in src)
            if (predicate(element))
                return false;

        return true;
    }

    public static SortedList<TKey, T> ToSortedList<TKey, T>(this IEnumerable<T> items, Func<T, TKey> getKey)
        where TKey : notnull
    {
        return new SortedList<TKey, T>(items.ToDictionary(getKey, x => x));
    }

    public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
    {
        IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>() };

        return sequences.Aggregate(
            emptyProduct,
            (accumulator, sequence) => from acc in accumulator from item in sequence select acc.Concat(new[] { item })
        );
    }
}
