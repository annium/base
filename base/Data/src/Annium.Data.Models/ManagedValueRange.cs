using System;

namespace Annium.Data.Models;

public sealed record ManagedValueRange<T>(
    T _start,
    T _end
) : ValueRange<T>
    where T : IComparable<T>
{
    public override T Start => _start;
    public override T End => _end;
    private T _start = _start;
    private T _end = _end;

    public void Set(T start, T end)
    {
        _start = start;
        _end = end;
    }

    public void SetStart(T start) => _start = start;
    public void SetEnd(T end) => _end = end;

    public override string ToString() => base.ToString();
}