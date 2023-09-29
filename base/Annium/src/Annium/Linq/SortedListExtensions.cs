using System.Collections.Generic;
using Annium.Collections.Generic;

namespace Annium.Linq;

public static class SortedListExtensions
{
    public static ISortedListSpan<TKey, TValue> ToListSpan<TKey, TValue>(
        this SortedList<TKey, TValue> items,
        int start,
        int count
    )
        where TKey : notnull =>
        new SortedListSpan<TKey, TValue>(items, start, count);
}