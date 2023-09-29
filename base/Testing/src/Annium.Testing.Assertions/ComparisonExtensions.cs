using System;
using System.Runtime.CompilerServices;
using Annium.Testing.Assertions.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Testing;

public static class ComparisonExtensions
{
    public static void IsLess<T>(
        this T value,
        T data,
        string? message = null,
        [CallerArgumentExpression("value")] string valueEx = default!,
        [CallerArgumentExpression("data")] string dataEx = default!
    )
        where T : IComparable<T>
    {
        if (value.CompareTo(data) >= 0)
            throw new AssertionFailedException(message ?? $"{value.Wrap(valueEx)} not < {data.Wrap(dataEx)}");
    }

    public static void IsLessOrEqual<T>(
        this T value,
        T data,
        string? message = null,
        [CallerArgumentExpression("value")] string valueEx = default!,
        [CallerArgumentExpression("data")] string dataEx = default!
    )
        where T : IComparable<T>
    {
        if (value.CompareTo(data) > 0)
            throw new AssertionFailedException(message ?? $"{value.Wrap(valueEx)} not <= {data.Wrap(dataEx)}");
    }

    public static void IsGreater<T>(
        this T value,
        T data,
        string? message = null,
        [CallerArgumentExpression("value")] string valueEx = default!,
        [CallerArgumentExpression("data")] string dataEx = default!
    )
        where T : IComparable<T>
    {
        if (value.CompareTo(data) <= 0)
            throw new AssertionFailedException(message ?? $"{value.Wrap(valueEx)} not > {data.Wrap(dataEx)}");
    }

    public static void IsGreaterOrEqual<T>(
        this T value,
        T data,
        string? message = null,
        [CallerArgumentExpression("value")] string valueEx = default!,
        [CallerArgumentExpression("data")] string dataEx = default!
    )
        where T : IComparable<T>
    {
        if (value.CompareTo(data) < 0)
            throw new AssertionFailedException(message ?? $"{value.Wrap(valueEx)} not >= {data.Wrap(dataEx)}");
    }
}