using System;
using System.Runtime.CompilerServices;

namespace Annium.Testing;

public static class ComparisonExtensions
{
    public static T IsLess<T>(
        this T value,
        T data,
        string? message = null,
        [CallerArgumentExpression("value")] string valueEx = default!,
        [CallerArgumentExpression("data")] string dataEx = default!
    )
        where T : IComparable<T>
    {
        if (value.CompareTo(data) >= 0)
            throw new AssertionFailedException(
                message ?? $"{value.WrapWithExpression(valueEx)} not < {data.WrapWithExpression(dataEx)}"
            );

        return value;
    }

    public static T IsLessOrEqual<T>(
        this T value,
        T data,
        string? message = null,
        [CallerArgumentExpression("value")] string valueEx = default!,
        [CallerArgumentExpression("data")] string dataEx = default!
    )
        where T : IComparable<T>
    {
        if (value.CompareTo(data) > 0)
            throw new AssertionFailedException(
                message ?? $"{value.WrapWithExpression(valueEx)} not <= {data.WrapWithExpression(dataEx)}"
            );

        return value;
    }

    public static T IsGreater<T>(
        this T value,
        T data,
        string? message = null,
        [CallerArgumentExpression("value")] string valueEx = default!,
        [CallerArgumentExpression("data")] string dataEx = default!
    )
        where T : IComparable<T>
    {
        if (value.CompareTo(data) <= 0)
            throw new AssertionFailedException(
                message ?? $"{value.WrapWithExpression(valueEx)} not > {data.WrapWithExpression(dataEx)}"
            );

        return value;
    }

    public static T IsGreaterOrEqual<T>(
        this T value,
        T data,
        string? message = null,
        [CallerArgumentExpression("value")] string valueEx = default!,
        [CallerArgumentExpression("data")] string dataEx = default!
    )
        where T : IComparable<T>
    {
        if (value.CompareTo(data) < 0)
            throw new AssertionFailedException(
                message ?? $"{value.WrapWithExpression(valueEx)} not >= {data.WrapWithExpression(dataEx)}"
            );

        return value;
    }
}
