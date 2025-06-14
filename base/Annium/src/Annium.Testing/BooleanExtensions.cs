using System.Runtime.CompilerServices;

namespace Annium.Testing;

/// <summary>
/// Provides extension methods for boolean assertions in tests.
/// </summary>
public static class BooleanExtensions
{
    /// <summary>
    /// Asserts that the boolean value is true.
    /// </summary>
    /// <param name="value">The boolean value to check.</param>
    /// <param name="message">Optional custom error message.</param>
    /// <param name="valueEx">The expression that produced the value.</param>
    /// <exception cref="AssertionFailedException">Thrown when the value is false.</exception>
    public static void IsTrue(
        this bool value,
        string? message = null,
        [CallerArgumentExpression(nameof(value))] string valueEx = ""
    )
    {
        if (!value)
            throw new AssertionFailedException(message ?? $"{value.WrapWithExpression(valueEx)} != True");
    }

    /// <summary>
    /// Asserts that the boolean value is false.
    /// </summary>
    /// <param name="value">The boolean value to check.</param>
    /// <param name="message">Optional custom error message.</param>
    /// <param name="valueEx">The expression that produced the value.</param>
    /// <exception cref="AssertionFailedException">Thrown when the value is true.</exception>
    public static void IsFalse(
        this bool value,
        string? message = null,
        [CallerArgumentExpression(nameof(value))] string valueEx = ""
    )
    {
        if (value)
            throw new AssertionFailedException(message ?? $"{value.WrapWithExpression(valueEx)} != False");
    }
}
