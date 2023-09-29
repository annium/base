using System.Collections.Generic;

namespace Annium.Linq;

public static class EnumerableExt
{
    public static IEnumerable<int> Range(int start, int count, int step)
    {
        for (var i = 0; i < count; i++)
            yield return start + i * step;
    }

    public static IEnumerable<decimal> Range(decimal start, int count, decimal step)
    {
        for (var i = 0; i < count; i++)
            yield return start + i * step;
    }
}