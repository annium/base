using System;

namespace Annium.Data.Models;

public sealed record ComputedValueRange<T>(
    Func<T> _getStart,
    Func<T> _getEnd
) : ValueRange<T>
    where T : IComparable<T>
{
    public override T Start => _getStart();
    public override T End => _getEnd();
    private readonly Func<T> _getStart = _getStart;
    private readonly Func<T> _getEnd = _getEnd;

    public override string ToString() => base.ToString();
}