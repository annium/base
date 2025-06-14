using System.Linq;
using System.Runtime.CompilerServices;

namespace Annium.Testing;

/// <summary>
/// Provides extension methods for string assertions in tests.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Asserts that the string contains the specified substring.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <param name="data">The substring to look for.</param>
    /// <param name="message">Optional custom error message.</param>
    /// <param name="valueEx">The expression that produced the string.</param>
    /// <param name="dataEx">The expression that produced the substring.</param>
    /// <returns>The original string.</returns>
    /// <exception cref="AssertionFailedException">Thrown when the string does not contain the substring.</exception>
    public static string IsContaining(
        this string value,
        string data,
        string? message = null,
        [CallerArgumentExpression(nameof(value))] string valueEx = "",
        [CallerArgumentExpression(nameof(data))] string dataEx = ""
    )
    {
        if (!value.Contains(data))
            throw new AssertionFailedException(
                message ?? $"{value.WrapWithExpression(valueEx)} doesn't contain {data.WrapWithExpression(dataEx)}"
            );

        return value;
    }

    /// <summary>
    /// Asserts that the string contains all specified substrings.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <param name="messages">The substrings to look for.</param>
    /// <param name="valueEx">The expression that produced the string.</param>
    /// <param name="messagesEx">The expression that produced the substrings.</param>
    /// <returns>The original string.</returns>
    /// <exception cref="AssertionFailedException">Thrown when the string does not contain all substrings.</exception>
    public static string IsContainingAll(
        this string value,
        string[] messages,
        [CallerArgumentExpression(nameof(value))] string valueEx = "",
        [CallerArgumentExpression(nameof(messages))] string messagesEx = ""
    )
    {
        if (!messages.All(value.Contains))
            throw new AssertionFailedException(
                $"{value.WrapWithExpression(valueEx)} expected to contain all: `{messages.WrapWithExpression(messagesEx)}`"
            );

        return value;
    }

    /// <summary>
    /// Asserts that the string does not contain the specified substring.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <param name="data">The substring to check for absence.</param>
    /// <param name="message">Optional custom error message.</param>
    /// <param name="valueEx">The expression that produced the string.</param>
    /// <param name="dataEx">The expression that produced the substring.</param>
    /// <returns>The original string.</returns>
    /// <exception cref="AssertionFailedException">Thrown when the string contains the substring.</exception>
    public static string IsNotContaining(
        this string value,
        string data,
        string? message = null,
        [CallerArgumentExpression(nameof(value))] string valueEx = "",
        [CallerArgumentExpression(nameof(data))] string dataEx = ""
    )
    {
        if (!value.Contains(data))
            throw new AssertionFailedException(
                message ?? $"{value.WrapWithExpression(valueEx)} contains {data.WrapWithExpression(dataEx)}"
            );
        return value;
    }
}
