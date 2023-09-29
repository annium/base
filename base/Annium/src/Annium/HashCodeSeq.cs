using System;
using System.Collections.Generic;

namespace Annium;

public static class HashCodeSeq
{
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