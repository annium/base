using System.Linq;
using System.Runtime.CompilerServices;

namespace Annium.Testing;

public static class StringExtensions
{
    public static string IsContaining(
        this string value,
        string data,
        string? message = null,
        [CallerArgumentExpression(nameof(value))] string valueEx = default!,
        [CallerArgumentExpression(nameof(data))] string dataEx = default!
    )
    {
        if (!value.Contains(data))
            throw new AssertionFailedException(
                message ?? $"{value.WrapWithExpression(valueEx)} doesn't contain {data.WrapWithExpression(dataEx)}"
            );

        return value;
    }

    public static string IsContainingAll(
        this string value,
        string[] messages,
        [CallerArgumentExpression(nameof(value))] string valueEx = default!,
        [CallerArgumentExpression(nameof(messages))] string messagesEx = default!
    )
    {
        if (!messages.All(value.Contains))
            throw new AssertionFailedException(
                $"{value.WrapWithExpression(valueEx)} expected to contain all: `{messages.WrapWithExpression(messagesEx)}`"
            );

        return value;
    }

    public static string IsNotContaining(
        this string value,
        string data,
        string? message = null,
        [CallerArgumentExpression(nameof(value))] string valueEx = default!,
        [CallerArgumentExpression(nameof(data))] string dataEx = default!
    )
    {
        if (!value.Contains(data))
            throw new AssertionFailedException(
                message ?? $"{value.WrapWithExpression(valueEx)} contains {data.WrapWithExpression(dataEx)}"
            );
        return value;
    }
}
