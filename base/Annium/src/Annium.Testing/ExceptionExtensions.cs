using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Annium.Testing;

/// <summary>
/// Provides extension methods for exception message assertions in tests.
/// </summary>
public static class ExceptionExtensions
{
    /// <summary>
    /// Asserts that the exception message contains the specified text.
    /// </summary>
    /// <typeparam name="T">The type of the exception.</typeparam>
    /// <param name="value">The exception to check.</param>
    /// <param name="message">The text that should be contained in the exception message.</param>
    /// <param name="valueEx">The expression that produced the exception.</param>
    /// <param name="messageEx">The expression that produced the message.</param>
    /// <returns>The original exception.</returns>
    /// <exception cref="AssertionFailedException">Thrown when the exception message does not contain the specified text.</exception>
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

    /// <summary>
    /// Asserts that the exception message contains all of the specified texts.
    /// </summary>
    /// <typeparam name="T">The type of the exception.</typeparam>
    /// <param name="value">The exception to check.</param>
    /// <param name="messages">The texts that should all be contained in the exception message.</param>
    /// <param name="valueEx">The expression that produced the exception.</param>
    /// <param name="messagesEx">The expression that produced the messages.</param>
    /// <returns>The original exception.</returns>
    /// <exception cref="AssertionFailedException">Thrown when the exception message does not contain all of the specified texts.</exception>
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

    /// <summary>
    /// Asserts that the exception message exactly matches the specified text.
    /// </summary>
    /// <typeparam name="T">The type of the exception.</typeparam>
    /// <param name="value">The exception to check.</param>
    /// <param name="message">The text that should exactly match the exception message.</param>
    /// <param name="valueEx">The expression that produced the exception.</param>
    /// <param name="messageEx">The expression that produced the message.</param>
    /// <returns>The original exception.</returns>
    /// <exception cref="AssertionFailedException">Thrown when the exception message does not exactly match the specified text.</exception>
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

    /// <summary>
    /// Asserts that the exception message from an asynchronous task contains the specified text.
    /// </summary>
    /// <typeparam name="T">The type of the exception.</typeparam>
    /// <param name="value">The task that produces the exception to check.</param>
    /// <param name="message">The text that should be contained in the exception message.</param>
    /// <param name="valueEx">The expression that produced the task.</param>
    /// <param name="messageEx">The expression that produced the message.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the original exception.</returns>
    /// <exception cref="AssertionFailedException">Thrown when the exception message does not contain the specified text.</exception>
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

    /// <summary>
    /// Asserts that the exception message from an asynchronous task contains all of the specified texts.
    /// </summary>
    /// <typeparam name="T">The type of the exception.</typeparam>
    /// <param name="value">The task that produces the exception to check.</param>
    /// <param name="messages">The texts that should all be contained in the exception message.</param>
    /// <param name="valueEx">The expression that produced the task.</param>
    /// <param name="messagesEx">The expression that produced the messages.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the original exception.</returns>
    /// <exception cref="AssertionFailedException">Thrown when the exception message does not contain all of the specified texts.</exception>
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

    /// <summary>
    /// Asserts that the exception message from an asynchronous task exactly matches the specified text.
    /// </summary>
    /// <typeparam name="T">The type of the exception.</typeparam>
    /// <param name="value">The task that produces the exception to check.</param>
    /// <param name="message">The text that should exactly match the exception message.</param>
    /// <param name="valueEx">The expression that produced the task.</param>
    /// <param name="messageEx">The expression that produced the message.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the original exception.</returns>
    /// <exception cref="AssertionFailedException">Thrown when the exception message does not exactly match the specified text.</exception>
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
