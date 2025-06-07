using System;

namespace Annium.Data.Models;

/// <summary>
/// A value range implementation with mutable start and end values
/// </summary>
/// <typeparam name="T">The type of values in the range</typeparam>
public sealed record ManagedValueRange<T>(T _start, T _end) : ValueRange<T>
    where T : IComparable<T>
{
    /// <summary>
    /// Gets the start value of the range
    /// </summary>
    public override T Start => _start;

    /// <summary>
    /// Gets the end value of the range
    /// </summary>
    public override T End => _end;

    /// <summary>
    /// The start value of the range.
    /// </summary>
    private T _start = _start;

    /// <summary>
    /// The end value of the range.
    /// </summary>
    private T _end = _end;

    /// <summary>
    /// Sets both the start and end values of the range
    /// </summary>
    /// <param name="start">The new start value</param>
    /// <param name="end">The new end value</param>
    public void Set(T start, T end)
    {
        _start = start;
        _end = end;
    }

    /// <summary>
    /// Sets the start value of the range
    /// </summary>
    /// <param name="start">The new start value</param>
    public void SetStart(T start) => _start = start;

    /// <summary>
    /// Sets the end value of the range
    /// </summary>
    /// <param name="end">The new end value</param>
    public void SetEnd(T end) => _end = end;

    /// <summary>
    /// Returns a string representation of the managed value range
    /// </summary>
    /// <returns>String representation of the range</returns>
    public override string ToString() => base.ToString();
}
