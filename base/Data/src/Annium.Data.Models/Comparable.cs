using System;
using System.Collections.Generic;

namespace Annium.Data.Models;

/// <summary>
/// Abstract base record class that provides comparison operations for derived types
/// </summary>
/// <typeparam name="T">The type that derives from Comparable</typeparam>
public abstract record Comparable<T> : IEquatable<T>, IComparable<T>, IComparable
    where T : Comparable<T>
{
    /// <summary>
    /// Compares this instance with another instance of the same type
    /// </summary>
    /// <param name="other">The instance to compare with</param>
    /// <returns>A value indicating the relative order of the instances</returns>
    public int CompareTo(T? other)
    {
        if (other is null)
            return 1;

        foreach (var accessor in GetComparables())
        {
            var value = Compare(accessor((T)this), accessor(other));
            if (value != 0)
                return value;
        }

        return 0;
    }

    /// <summary>
    /// Compares this instance with another object
    /// </summary>
    /// <param name="obj">The object to compare with</param>
    /// <returns>A value indicating the relative order of the instances</returns>
    public int CompareTo(object? obj)
    {
        if (obj is T other)
            return CompareTo(other);
        throw new ArgumentException($"Cannot compare {typeof(T)} with {obj?.GetType().FullName ?? "null"}");
    }

    /// <summary>
    /// Gets the collection of property accessors used for comparison
    /// </summary>
    /// <returns>Collection of functions that extract comparable values</returns>
    protected abstract IEnumerable<Func<T, IComparable>> GetComparables();

    /// <summary>
    /// Determines if the first instance is greater than the second
    /// </summary>
    /// <param name="a">First instance</param>
    /// <param name="b">Second instance</param>
    /// <returns>True if first instance is greater than second</returns>
    public static bool operator >(Comparable<T> a, Comparable<T> b) => Compare(a, b) == 1;

    /// <summary>
    /// Determines if the first instance is less than the second
    /// </summary>
    /// <param name="a">First instance</param>
    /// <param name="b">Second instance</param>
    /// <returns>True if first instance is less than second</returns>
    public static bool operator <(Comparable<T> a, Comparable<T> b) => Compare(a, b) == -1;

    /// <summary>
    /// Determines if the first instance is greater than or equal to the second
    /// </summary>
    /// <param name="a">First instance</param>
    /// <param name="b">Second instance</param>
    /// <returns>True if first instance is greater than or equal to second</returns>
    public static bool operator >=(Comparable<T> a, Comparable<T> b) => Compare(a, b) >= 0;

    /// <summary>
    /// Determines if the first instance is less than or equal to the second
    /// </summary>
    /// <param name="a">First instance</param>
    /// <param name="b">Second instance</param>
    /// <returns>True if first instance is less than or equal to second</returns>
    public static bool operator <=(Comparable<T> a, Comparable<T> b) => Compare(a, b) <= 0;

    /// <summary>
    /// Compares two IComparable objects handling null values.
    /// </summary>
    /// <param name="a">The first object to compare.</param>
    /// <param name="b">The second object to compare.</param>
    /// <returns>A value indicating the relative order of the objects.</returns>
    private static int Compare(IComparable? a, IComparable? b)
    {
        if (ReferenceEquals(a, b))
            return 0;
        if (a is null)
            return -1;
        if (b is null)
            return 1;

        return a.CompareTo(b);
    }

    /// <summary>
    /// Determines if this instance is equal to another instance
    /// </summary>
    /// <param name="other">The instance to compare with</param>
    /// <returns>True if instances are equal</returns>
    public bool Equals(T? other) => GetHashCode() == other?.GetHashCode();
}
