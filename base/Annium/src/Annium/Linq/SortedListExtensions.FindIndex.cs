using System;
using System.Collections.Generic;

namespace Annium.Linq;

public static class SortedListFindIndexExtensions
{
    public static int FindIndex<TKey, TValue>(
        this SortedList<TKey, TValue> source,
        TKey key
    )
        where TKey : notnull
    {
        var i = 0;

        foreach (var pair in source)
            if (pair.Key.Equals(key))
                return i;
            else
                i++;

        return -1;
    }

    public static int FindIndex<TKey, TValue>(
        this SortedList<TKey, TValue> source,
        Predicate<TValue> match
    )
        where TKey : notnull
    {
        var i = 0;

        foreach (var pair in source)
            if (match(pair.Value))
                return i;
            else
                i++;

        return -1;
    }
}