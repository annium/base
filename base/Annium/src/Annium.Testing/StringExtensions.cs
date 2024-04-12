using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Annium.Testing.Internal;

namespace Annium.Testing;

public static class StringExtensions
{
    public static void IsContaining(
        this string value,
        string data,
        string? message = null,
        [CallerArgumentExpression("value")] string valueEx = default!,
        [CallerArgumentExpression("data")] string dataEx = default!
    )
    {
        if (!value.Contains(data))
            throw new AssertionFailedException(message ?? $"{value.Wrap(valueEx)} doesn't contain {data.Wrap(dataEx)}");
    }

    public static void IsNotContaining(
        this string value,
        string data,
        string? message = null,
        [CallerArgumentExpression("value")] string valueEx = default!,
        [CallerArgumentExpression("data")] string dataEx = default!
    )
    {
        if (!value.Contains(data))
            throw new AssertionFailedException(message ?? $"{value.Wrap(valueEx)} contains {data.Wrap(dataEx)}");
    }
}
