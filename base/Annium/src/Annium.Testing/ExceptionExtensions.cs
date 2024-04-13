using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Annium.Testing;

public static class ExceptionExtensions
{
    public static T Reports<T>(
        this T value,
        string message,
        [CallerArgumentExpression("value")] string valueEx = default!,
        [CallerArgumentExpression("message")] string messageEx = default!
    )
        where T : Exception
    {
        if (!value.Message.Contains(message))
            throw new AssertionFailedException(
                $"{value.Message.WrapWithExpression(valueEx)} expected to report: `{message.WrapWithExpression(messageEx)}`"
            );

        return value;
    }

    public static T ReportsAll<T>(
        this T value,
        string[] messages,
        [CallerArgumentExpression("value")] string valueEx = default!,
        [CallerArgumentExpression("messages")] string messagesEx = default!
    )
        where T : Exception
    {
        if (!messages.All(value.Message.Contains))
            throw new AssertionFailedException(
                $"{value.Message.WrapWithExpression(valueEx)} expected to report: `{messages.WrapWithExpression(messagesEx)}`"
            );

        return value;
    }

    public static T ReportsExactly<T>(
        this T value,
        string message,
        [CallerArgumentExpression("value")] string valueEx = default!,
        [CallerArgumentExpression("message")] string messageEx = default!
    )
        where T : Exception
    {
        value.Message.Is(
            message,
            $"{value.Message.WrapWithExpression(valueEx)} expected to report: `{message.WrapWithExpression(messageEx)}`"
        );

        return value;
    }
}
