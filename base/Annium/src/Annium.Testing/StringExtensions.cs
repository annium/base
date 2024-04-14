using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

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
            throw new AssertionFailedException(
                message ?? $"{value.WrapWithExpression(valueEx)} doesn't contain {data.WrapWithExpression(dataEx)}"
            );
    }

    public static void IsContainingAll(
        this string value,
        string[] messages,
        [CallerArgumentExpression("value")] string valueEx = default!,
        [CallerArgumentExpression("messages")] string messagesEx = default!
    )
    {
        if (!messages.All(value.Contains))
            throw new AssertionFailedException(
                $"{value.WrapWithExpression(valueEx)} expected to contain all: `{messages.WrapWithExpression(messagesEx)}`"
            );
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
            throw new AssertionFailedException(
                message ?? $"{value.WrapWithExpression(valueEx)} contains {data.WrapWithExpression(dataEx)}"
            );
    }
}
