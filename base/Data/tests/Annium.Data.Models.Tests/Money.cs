using System;
using System.Collections.Generic;

namespace Annium.Data.Models.Tests;

/// <summary>
/// Test class representing a money value with major and minor components, implementing comparable functionality.
/// </summary>
public class Money : Comparable<Money>
{
    /// <summary>
    /// Gets the major component of the money value.
    /// </summary>
    public int Major { get; }

    /// <summary>
    /// Gets the minor component of the money value.
    /// </summary>
    public int Minor { get; }

    /// <summary>
    /// Initializes a new instance of the Money class.
    /// </summary>
    /// <param name="major">The major component of the money value.</param>
    /// <param name="minor">The minor component of the money value.</param>
    public Money(int major, int minor)
    {
        Major = major;
        Minor = minor;
    }

    /// <summary>
    /// Gets the comparable functions used for comparison operations.
    /// </summary>
    /// <returns>An enumerable of functions that extract comparable values from Money instances.</returns>
    protected override IEnumerable<Func<Money, IComparable>> GetComparables()
    {
        yield return x => x.Major;
        yield return x => x.Minor;
    }

    /// <summary>
    /// Returns the hash code for this Money instance.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode() => HashCode.Combine(Major, Minor);
}
