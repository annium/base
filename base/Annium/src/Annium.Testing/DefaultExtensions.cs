using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Annium.Testing;

/// <summary>
/// Provides extension methods for default value assertions in tests.
/// </summary>
public static class ValueExtensions
{
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
        value.Is(default, $"{value.WrapWithExpression(valueEx)} is not default");

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
        value.IsNot(default, $"{value.WrapWithExpression(valueEx)} is default");
        ArgumentNullException.ThrowIfNull(value);

        return value;
    }
}
