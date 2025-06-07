using System;

namespace Annium.Data.Models;

/// <summary>
/// A value range implementation that computes start and end values using functions
/// </summary>
/// <typeparam name="T">The type of values in the range</typeparam>
public sealed record ComputedValueRange<T>(Func<T> _getStart, Func<T> _getEnd) : ValueRange<T>
    where T : IComparable<T>
{
    /// <summary>
    /// Gets the start value of the range by invoking the start function
    /// </summary>
    public override T Start => _getStart();

    /// <summary>
    /// Gets the end value of the range by invoking the end function
    /// </summary>
    public override T End => _getEnd();

    /// <summary>
    /// Function to compute the start value of the range.
    /// </summary>
    private readonly Func<T> _getStart = _getStart;

    /// <summary>
    /// Function to compute the end value of the range.
    /// </summary>
    private readonly Func<T> _getEnd = _getEnd;

    /// <summary>
    /// Returns a string representation of the computed value range
    /// </summary>
    /// <returns>String representation of the range</returns>
    public override string ToString() => base.ToString();
}
