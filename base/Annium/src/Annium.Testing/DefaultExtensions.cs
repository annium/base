using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Annium.Testing;

/// <summary>
/// Provides extension methods for default value assertions in tests.
/// </summary>
public static class DefaultExtensions
{
    /// <summary>
    /// Asserts that the value is null.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="message">Optional custom error message.</param>
    /// <param name="valueEx">The expression that produced the value.</param>
    /// <exception cref="AssertionFailedException">Thrown when the value is not null.</exception>
    public static void IsNull<T>(
        this T? value,
        string? message = null,
        [CallerArgumentExpression(nameof(value))] string valueEx = ""
    )
        where T : class
    {
        if (value is not null)
            throw new AssertionFailedException(message ?? $"{valueEx} is not null");
    }

    /// <summary>
    /// Asserts that the value is null (struct nullable overload).
    /// </summary>
    /// <typeparam name="T">The underlying value type.</typeparam>
    /// <param name="value">The nullable value to check.</param>
    /// <param name="message">Optional custom error message.</param>
    /// <param name="valueEx">The expression that produced the value.</param>
    /// <exception cref="AssertionFailedException">Thrown when the value is not null.</exception>
    public static void IsNull<T>(
        this T? value,
        string? message = null,
        [CallerArgumentExpression(nameof(value))] string valueEx = ""
    )
        where T : struct
    {
        if (value.HasValue)
            throw new AssertionFailedException(message ?? $"{valueEx} is not null");
    }

    /// <summary>
    /// Asserts that the value is not null and returns the non-null value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="message">Optional custom error message.</param>
    /// <param name="valueEx">The expression that produced the value.</param>
    /// <returns>The non-null value.</returns>
    /// <exception cref="AssertionFailedException">Thrown when the value is null.</exception>
    [return: NotNull]
    public static T IsNotNull<T>(
        [NotNull] this T? value,
        string? message = null,
        [CallerArgumentExpression(nameof(value))] string valueEx = ""
    )
        where T : class
    {
        if (value is null)
            throw new AssertionFailedException(message ?? $"{valueEx} is null");

        return value;
    }

    /// <summary>
    /// Asserts that the nullable struct value is not null and returns the underlying value.
    /// </summary>
    /// <typeparam name="T">The underlying value type.</typeparam>
    /// <param name="value">The nullable value to check.</param>
    /// <param name="message">Optional custom error message.</param>
    /// <param name="valueEx">The expression that produced the value.</param>
    /// <returns>The non-null underlying value.</returns>
    /// <exception cref="AssertionFailedException">Thrown when the value is null.</exception>
    public static T IsNotNull<T>(
        this T? value,
        string? message = null,
        [CallerArgumentExpression(nameof(value))] string valueEx = ""
    )
        where T : struct
    {
        if (!value.HasValue)
            throw new AssertionFailedException(message ?? $"{valueEx} is null");

        return value.Value;
    }

    /// <summary>
    /// Asserts that the value is the default value for its type.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="message">Optional custom error message.</param>
    /// <param name="valueEx">The expression that produced the value.</param>
    /// <returns>The original value.</returns>
    /// <exception cref="AssertionFailedException">Thrown when the value is not the default value.</exception>
    public static T IsDefault<T>(
        this T value,
        string? message = null,
        [CallerArgumentExpression(nameof(value))] string valueEx = ""
    )
    {
        value.Is(default, message ?? $"{value.WrapWithExpression(valueEx)} is not default");

        return value;
    }

    /// <summary>
    /// Asserts that the value is not the default value for its type.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="message">Optional custom error message.</param>
    /// <param name="valueEx">The expression that produced the value.</param>
    /// <returns>The original value.</returns>
    /// <exception cref="AssertionFailedException">Thrown when the value is the default value.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the value is null.</exception>
    [return: NotNull]
    public static T IsNotDefault<T>(
        [NotNull] this T value,
        string? message = null,
        [CallerArgumentExpression(nameof(value))] string valueEx = ""
    )
    {
        ArgumentNullException.ThrowIfNull(value);
        value.IsNot(default, message ?? $"{value.WrapWithExpression(valueEx)} is default");

        return value;
    }
}
