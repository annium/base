using System.Runtime.CompilerServices;

namespace Annium.Testing;

/// <summary>
/// Provides extension methods for type assertions in tests.
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    /// Asserts that the object is of the specified type and returns it cast to that type.
    /// </summary>
    /// <typeparam name="T">The expected type.</typeparam>
    /// <param name="value">The object to check.</param>
    /// <param name="message">Optional custom error message.</param>
    /// <param name="valueEx">The expression that produced the object.</param>
    /// <returns>The object cast to the specified type.</returns>
    /// <exception cref="AssertionFailedException">Thrown when the object is not of the specified type.</exception>
    public static T As<T>(
        this object? value,
        string? message = null,
        [CallerArgumentExpression(nameof(value))] string valueEx = ""
    )
    {
        (value is T).IsTrue(message ?? $"{valueEx} is {value?.GetType()}, not {typeof(T)}");

        return (T)value!;
    }

    /// <summary>
    /// Asserts that the object is exactly of the specified type and returns it cast to that type.
    /// </summary>
    /// <typeparam name="T">The expected type.</typeparam>
    /// <param name="value">The object to check.</param>
    /// <param name="message">Optional custom error message.</param>
    /// <param name="valueEx">The expression that produced the object.</param>
    /// <returns>The object cast to the specified type.</returns>
    /// <exception cref="AssertionFailedException">Thrown when the object is not exactly of the specified type.</exception>
    public static T AsExact<T>(
        this object? value,
        string? message = null,
        [CallerArgumentExpression(nameof(value))] string valueEx = ""
    )
    {
        (value?.GetType() == typeof(T)).IsTrue(message ?? $"{valueEx} is {value?.GetType()}, not {typeof(T)}");

        return (T)value!;
    }
}
