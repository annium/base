using System;
using System.Collections.Generic;

namespace Annium.Data.Models;

/// <summary>
/// Abstract base class representing a range of comparable values
/// </summary>
/// <typeparam name="T">The type of values in the range</typeparam>
public abstract record ValueRange<T>
    where T : IComparable<T>
{
    /// <summary>
    /// Gets the start value of the range
    /// </summary>
    public abstract T Start { get; }

    /// <summary>
    /// Gets the end value of the range
    /// </summary>
    public abstract T End { get; }

    /// <summary>
    /// Determines whether the range contains the specified value
    /// </summary>
    /// <param name="value">The value to check</param>
    /// <param name="bounds">The boundary inclusion rules</param>
    /// <returns>True if the range contains the value</returns>
    public bool Contains(T value, RangeBounds bounds) =>
        bounds switch
        {
            RangeBounds.None => Start.CompareTo(value) < 0 && End.CompareTo(value) > 0,
            RangeBounds.Start => Start.CompareTo(value) <= 0 && End.CompareTo(value) > 0,
            RangeBounds.End => Start.CompareTo(value) < 0 && End.CompareTo(value) >= 0,
            RangeBounds.Both => Start.CompareTo(value) <= 0 && End.CompareTo(value) >= 0,
            _ => throw new ArgumentOutOfRangeException(nameof(bounds), bounds, null),
        };

    /// <summary>
    /// Determines whether this range contains another range
    /// </summary>
    /// <param name="value">The range to check</param>
    /// <param name="bounds">The boundary inclusion rules</param>
    /// <returns>True if this range contains the other range</returns>
    public bool Contains(ValueRange<T> value, RangeBounds bounds) =>
        bounds switch
        {
            RangeBounds.None => Start.CompareTo(value.Start) < 0 && End.CompareTo(value.End) > 0,
            RangeBounds.Start => Start.CompareTo(value.Start) <= 0 && End.CompareTo(value.End) > 0,
            RangeBounds.End => Start.CompareTo(value.Start) < 0 && End.CompareTo(value.End) >= 0,
            RangeBounds.Both => Start.CompareTo(value.Start) <= 0 && End.CompareTo(value.End) >= 0,
            _ => throw new ArgumentOutOfRangeException(nameof(bounds), bounds, null),
        };

    /// <summary>
    /// Deconstructs the range into its start and end values
    /// </summary>
    /// <param name="start">The start value</param>
    /// <param name="end">The end value</param>
    public void Deconstruct(out T start, out T end)
    {
        start = Start;
        end = End;
    }

    /// <summary>
    /// Returns a string representation of the range
    /// </summary>
    /// <returns>String representation in the format "start - end"</returns>
    public override string ToString() => $"{Start} - {End}";

    /// <summary>
    /// Determines whether this range is equal to another range
    /// </summary>
    /// <param name="other">The range to compare with</param>
    /// <returns>True if the ranges are equal</returns>
    public virtual bool Equals(ValueRange<T>? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return EqualityComparer<T>.Default.Equals(Start, other.Start)
            && EqualityComparer<T>.Default.Equals(End, other.End);
    }

    /// <summary>
    /// Returns the hash code for this range
    /// </summary>
    /// <returns>Hash code based on start and end values</returns>
    public override int GetHashCode() => HashCode.Combine(Start, End);

    /// <summary>
    /// Subtracts one range from another, returning the remaining ranges
    /// </summary>
    /// <param name="src">The source range</param>
    /// <param name="tgt">The range to subtract</param>
    /// <returns>Collection of remaining ranges after subtraction</returns>
    public static IReadOnlyCollection<ValueRange<T>> operator -(ValueRange<T> src, ValueRange<T> tgt)
    {
        // SS TS TE SE -> SS TS, TE SE
        if (src.Contains(tgt.Start, RangeBounds.None) && src.Contains(tgt.End, RangeBounds.None))
            return new[] { ValueRange.Create(src.Start, tgt.Start), ValueRange.Create(tgt.End, src.End) };

        var startInRange = tgt.Contains(src.Start, RangeBounds.Both);
        var endInRange = tgt.Contains(src.End, RangeBounds.Both);

        // TS SS SE TE -> []
        if (startInRange && endInRange)
            return Array.Empty<ValueRange<T>>();

        // TS SS TE SE-> TE SE
        if (startInRange && !endInRange)
            return new[] { ValueRange.Create(tgt.End, src.End) };

        // SS TS SE TE -> SS TS
        if (!startInRange && endInRange)
            return new[] { ValueRange.Create(src.Start, tgt.Start) };

        // SS SE TS TE -> SS SE
        // TS TE SS SE -> SS SE
        return new[] { src };
    }
}
