using System;
using System.Collections.Generic;

namespace Annium.Data.Models;

public abstract record ValueRange<T>
    where T : IComparable<T>
{
    public abstract T Start { get; }
    public abstract T End { get; }

    public bool Contains(T value, RangeBounds bounds) => bounds switch
    {
        RangeBounds.None  => Start.CompareTo(value) < 0 && End.CompareTo(value) > 0,
        RangeBounds.Start => Start.CompareTo(value) <= 0 && End.CompareTo(value) > 0,
        RangeBounds.End   => Start.CompareTo(value) < 0 && End.CompareTo(value) >= 0,
        RangeBounds.Both  => Start.CompareTo(value) <= 0 && End.CompareTo(value) >= 0,
        _                 => throw new ArgumentOutOfRangeException(nameof(bounds), bounds, null)
    };

    public bool Contains(ValueRange<T> value, RangeBounds bounds) => bounds switch
    {
        RangeBounds.None  => Start.CompareTo(value.Start) < 0 && End.CompareTo(value.End) > 0,
        RangeBounds.Start => Start.CompareTo(value.Start) <= 0 && End.CompareTo(value.End) > 0,
        RangeBounds.End   => Start.CompareTo(value.Start) < 0 && End.CompareTo(value.End) >= 0,
        RangeBounds.Both  => Start.CompareTo(value.Start) <= 0 && End.CompareTo(value.End) >= 0,
        _                 => throw new ArgumentOutOfRangeException(nameof(bounds), bounds, null)
    };

    public void Deconstruct(out T start, out T end)
    {
        start = Start;
        end = End;
    }

    public override string ToString() => $"{Start} - {End}";

    public virtual bool Equals(ValueRange<T>? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return EqualityComparer<T>.Default.Equals(Start, other.Start) && EqualityComparer<T>.Default.Equals(End, other.End);
    }

    public override int GetHashCode() => HashCode.Combine(Start, End);

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