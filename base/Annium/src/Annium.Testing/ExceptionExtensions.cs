using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Annium.Testing;

public static class ExceptionExtensions
{
    public static T Reports<T>(
        this T value,
        string message,
        [CallerArgumentExpression(nameof(value))] string valueEx = default!,
        [CallerArgumentExpression(nameof(message))] string messageEx = default!
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
        [CallerArgumentExpression(nameof(value))] string valueEx = default!,
        [CallerArgumentExpression(nameof(messages))] string messagesEx = default!
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
        [CallerArgumentExpression(nameof(value))] string valueEx = default!,
        [CallerArgumentExpression(nameof(message))] string messageEx = default!
    )
        where T : Exception
    {
        value.Message.Is(
            message,
            $"{value.Message.WrapWithExpression(valueEx)} expected to report: `{message.WrapWithExpression(messageEx)}`"
        );

        return value;
    }

    public static async Task<T> ReportsAsync<T>(
        this Task<T> value,
        string message,
        [CallerArgumentExpression(nameof(value))] string valueEx = default!,
        [CallerArgumentExpression(nameof(message))] string messageEx = default!
    )
        where T : Exception
    {
#pragma warning disable VSTHRD003
        var val = await value;
#pragma warning restore VSTHRD003

        if (!val.Message.Contains(message))
            throw new AssertionFailedException(
                $"{val.Message.WrapWithExpression(valueEx)} expected to report: `{message.WrapWithExpression(messageEx)}`"
            );

        return val;
    }

    public static async Task<T> ReportsAllAsync<T>(
        this Task<T> value,
        string[] messages,
        [CallerArgumentExpression(nameof(value))] string valueEx = default!,
        [CallerArgumentExpression(nameof(messages))] string messagesEx = default!
    )
        where T : Exception
    {
#pragma warning disable VSTHRD003
        var val = await value;
#pragma warning restore VSTHRD003

        if (!messages.All(val.Message.Contains))
            throw new AssertionFailedException(
                $"{val.Message.WrapWithExpression(valueEx)} expected to report: `{messages.WrapWithExpression(messagesEx)}`"
            );

        return val;
    }

    public static async Task<T> ReportsExactlyAsync<T>(
        this Task<T> value,
        string message,
        [CallerArgumentExpression(nameof(value))] string valueEx = default!,
        [CallerArgumentExpression(nameof(message))] string messageEx = default!
    )
        where T : Exception
    {
#pragma warning disable VSTHRD003
        var val = await value;
#pragma warning restore VSTHRD003

        val.Message.Is(
            message,
            $"{val.Message.WrapWithExpression(valueEx)} expected to report: `{message.WrapWithExpression(messageEx)}`"
        );

        return val;
    }
}
