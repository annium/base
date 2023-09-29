using System.Collections.Generic;
using Annium.Collections.Generic;

namespace Annium.Linq;

public static class ReadOnlyListExtensions
{
    public static IListSpan<T> ToListSpan<T>(this IReadOnlyList<T> items, int start, int count) =>
        new ListSpan<T>(items, start, count);
}