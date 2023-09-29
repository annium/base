using System;
using System.Collections.Generic;

namespace Annium.Data.Models;

public abstract class Comparable<T> : IEquatable<T>, IComparable<T>, IComparable where T : Comparable<T>
{
    public int CompareTo(T? other)
    {
        if (other is null) return 1;

        foreach (var accessor in GetComparables())
        {
            var value = Compare(accessor((T)this), accessor(other));
            if (value != 0)
                return value;
        }

        return 0;
    }

    public int CompareTo(object? obj)
    {
        if (obj is T other)
            return CompareTo(other);
        throw new ArgumentException($"Cannot compare {typeof(T)} with {obj?.GetType().FullName ?? "null"}");
    }

    protected abstract IEnumerable<Func<T, IComparable>> GetComparables();

    public static bool operator >(Comparable<T> a, Comparable<T> b) => Compare(a, b) == 1;

    public static bool operator <(Comparable<T> a, Comparable<T> b) => Compare(a, b) == -1;

    public static bool operator >=(Comparable<T> a, Comparable<T> b) => Compare(a, b) >= 0;

    public static bool operator <=(Comparable<T> a, Comparable<T> b) => Compare(a, b) <= 0;

    private static int Compare(IComparable? a, IComparable? b)
    {
        if (ReferenceEquals(a, b)) return 0;
        if (a is null) return -1;
        if (b is null) return 1;

        return a.CompareTo(b);
    }

    public bool Equals(T? other) => GetHashCode() == other?.GetHashCode();
}