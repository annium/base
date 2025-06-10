using System;

namespace Annium.Data.Models;

/// <summary>
/// Factory class for creating value range instances
/// </summary>
public static class ValueRange
{
    /// <summary>
    /// Creates a managed value range with the specified start and end values
    /// </summary>
    /// <typeparam name="T">The type of values in the range</typeparam>
    /// <param name="start">The start value</param>
    /// <param name="end">The end value</param>
    /// <returns>A new managed value range</returns>
    public static ManagedValueRange<T> Create<T>(T start, T end)
        where T : IComparable<T>
    {
        return new ManagedValueRange<T>(start, end);
    }

    /// <summary>
    /// Creates a computed value range with the specified start and end functions
    /// </summary>
    /// <typeparam name="T">The type of values in the range</typeparam>
    /// <param name="start">Function that returns the start value</param>
    /// <param name="end">Function that returns the end value</param>
    /// <returns>A new computed value range</returns>
    public static ComputedValueRange<T> Create<T>(Func<T> start, Func<T> end)
        where T : IComparable<T>
    {
        return new ComputedValueRange<T>(start, end);
    }
}
