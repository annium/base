using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Annium.Testing;

public static class EqualityExtensions
{
    public static void Is<T>(
        this T value,
        T data,
        string? message = null,
        [CallerArgumentExpression("value")] string valueEx = default!,
        [CallerArgumentExpression("data")] string dataEx = default!
    )
    {
        if (!EqualityComparer<T>.Default.Equals(value, data))
            throw new AssertionFailedException(
                message ?? $"{value.WrapWithExpression(valueEx)} != {data.WrapWithExpression(dataEx)}"
            );
    }

    public static void IsNot<T>(
        this T value,
        T data,
        string? message = null,
        [CallerArgumentExpression("value")] string valueEx = default!,
        [CallerArgumentExpression("data")] string dataEx = default!
    )
    {
        if (EqualityComparer<T>.Default.Equals(value, data))
            throw new AssertionFailedException(
                message ?? $"{value.WrapWithExpression(valueEx)} == {data.WrapWithExpression(dataEx)}"
            );
    }
}
