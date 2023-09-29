using System;

namespace Annium.Data.Models;

public static class ValueRange
{
    public static ManagedValueRange<T> Create<T>(T start, T end)
        where T : IComparable<T>
    {
        return new ManagedValueRange<T>(start, end);
    }

    public static ComputedValueRange<T> Create<T>(Func<T> start, Func<T> end)
        where T : IComparable<T>
    {
        return new ComputedValueRange<T>(start, end);
    }
}