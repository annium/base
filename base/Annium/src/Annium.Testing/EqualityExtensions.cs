using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Annium.Testing;

/// <summary>
/// Provides extension methods for equality assertions in tests.
/// </summary>
public static class EqualityExtensions
{
    /// <summary>
    /// Asserts that the value is equal to the specified data.
    /// </summary>
    /// <typeparam name="T">The type of the values being compared.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="data">The value to compare against.</param>
    /// <param name="message">Optional custom error message.</param>
    /// <param name="valueEx">The expression that produced the value.</param>
    /// <param name="dataEx">The expression that produced the data.</param>
    /// <returns>The original value.</returns>
    /// <exception cref="AssertionFailedException">Thrown when the value is not equal to the data.</exception>
    public static T Is<T>(
        this T value,
        T data,
        string? message = null,
        [CallerArgumentExpression(nameof(value))] string valueEx = "",
        [CallerArgumentExpression(nameof(data))] string dataEx = ""
    )
    {
        if (!EqualityComparer<T>.Default.Equals(value, data))
            throw new AssertionFailedException(
                message ?? $"{value.WrapWithExpression(valueEx)} != {data.WrapWithExpression(dataEx)}"
            );

        return value;
    }

    /// <summary>
    /// Asserts that the value is not equal to the specified data.
    /// </summary>
    /// <typeparam name="T">The type of the values being compared.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="data">The value to compare against.</param>
    /// <param name="message">Optional custom error message.</param>
    /// <param name="valueEx">The expression that produced the value.</param>
    /// <param name="dataEx">The expression that produced the data.</param>
    /// <returns>The original value.</returns>
    /// <exception cref="AssertionFailedException">Thrown when the value is equal to the data.</exception>
    public static T IsNot<T>(
        this T value,
        T data,
        string? message = null,
        [CallerArgumentExpression(nameof(value))] string valueEx = "",
        [CallerArgumentExpression(nameof(data))] string dataEx = ""
    )
    {
        if (EqualityComparer<T>.Default.Equals(value, data))
            throw new AssertionFailedException(
                message ?? $"{value.WrapWithExpression(valueEx)} == {data.WrapWithExpression(dataEx)}"
            );

        return value;
    }
}
