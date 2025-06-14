using System;
using System.Runtime.CompilerServices;

namespace Annium.Testing;

/// <summary>
/// Provides extension methods for comparison assertions in tests.
/// </summary>
public static class ComparisonExtensions
{
    /// <summary>
    /// Asserts that the value is less than the specified data.
    /// </summary>
    /// <typeparam name="T">The type of the values being compared.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="data">The value to compare against.</param>
    /// <param name="message">Optional custom error message.</param>
    /// <param name="valueEx">The expression that produced the value.</param>
    /// <param name="dataEx">The expression that produced the data.</param>
    /// <returns>The original value.</returns>
    /// <exception cref="AssertionFailedException">Thrown when the value is not less than the data.</exception>
    public static T IsLess<T>(
        this T value,
        T data,
        string? message = null,
        [CallerArgumentExpression(nameof(value))] string valueEx = "",
        [CallerArgumentExpression(nameof(data))] string dataEx = ""
    )
        where T : IComparable<T>
    {
        if (value.CompareTo(data) >= 0)
            throw new AssertionFailedException(
                message ?? $"{value.WrapWithExpression(valueEx)} not < {data.WrapWithExpression(dataEx)}"
            );

        return value;
    }

    /// <summary>
    /// Asserts that the value is less than or equal to the specified data.
    /// </summary>
    /// <typeparam name="T">The type of the values being compared.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="data">The value to compare against.</param>
    /// <param name="message">Optional custom error message.</param>
    /// <param name="valueEx">The expression that produced the value.</param>
    /// <param name="dataEx">The expression that produced the data.</param>
    /// <returns>The original value.</returns>
    /// <exception cref="AssertionFailedException">Thrown when the value is not less than or equal to the data.</exception>
    public static T IsLessOrEqual<T>(
        this T value,
        T data,
        string? message = null,
        [CallerArgumentExpression(nameof(value))] string valueEx = "",
        [CallerArgumentExpression(nameof(data))] string dataEx = ""
    )
        where T : IComparable<T>
    {
        if (value.CompareTo(data) > 0)
            throw new AssertionFailedException(
                message ?? $"{value.WrapWithExpression(valueEx)} not <= {data.WrapWithExpression(dataEx)}"
            );

        return value;
    }

    /// <summary>
    /// Asserts that the value is greater than the specified data.
    /// </summary>
    /// <typeparam name="T">The type of the values being compared.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="data">The value to compare against.</param>
    /// <param name="message">Optional custom error message.</param>
    /// <param name="valueEx">The expression that produced the value.</param>
    /// <param name="dataEx">The expression that produced the data.</param>
    /// <returns>The original value.</returns>
    /// <exception cref="AssertionFailedException">Thrown when the value is not greater than the data.</exception>
    public static T IsGreater<T>(
        this T value,
        T data,
        string? message = null,
        [CallerArgumentExpression(nameof(value))] string valueEx = "",
        [CallerArgumentExpression(nameof(data))] string dataEx = ""
    )
        where T : IComparable<T>
    {
        if (value.CompareTo(data) <= 0)
            throw new AssertionFailedException(
                message ?? $"{value.WrapWithExpression(valueEx)} not > {data.WrapWithExpression(dataEx)}"
            );

        return value;
    }

    /// <summary>
    /// Asserts that the value is greater than or equal to the specified data.
    /// </summary>
    /// <typeparam name="T">The type of the values being compared.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="data">The value to compare against.</param>
    /// <param name="message">Optional custom error message.</param>
    /// <param name="valueEx">The expression that produced the value.</param>
    /// <param name="dataEx">The expression that produced the data.</param>
    /// <returns>The original value.</returns>
    /// <exception cref="AssertionFailedException">Thrown when the value is not greater than or equal to the data.</exception>
    public static T IsGreaterOrEqual<T>(
        this T value,
        T data,
        string? message = null,
        [CallerArgumentExpression(nameof(value))] string valueEx = "",
        [CallerArgumentExpression(nameof(data))] string dataEx = ""
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
