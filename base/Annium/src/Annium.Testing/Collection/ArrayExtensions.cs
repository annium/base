using System;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace Annium.Testing;

/// <summary>
/// Provides extension methods for array assertions in tests.
/// </summary>
public static class ArrayExtensions
{
    /// <summary>
    /// Asserts that the array has an element at the specified index and returns it.
    /// </summary>
    /// <typeparam name="T">The type of elements in the array.</typeparam>
    /// <param name="value">The array to check.</param>
    /// <param name="key">The index to check.</param>
    /// <param name="valueEx">The expression that produced the array.</param>
    /// <param name="keyEx">The expression that produced the index.</param>
    /// <returns>The element at the specified index.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the array is null.</exception>
    /// <exception cref="AssertionFailedException">Thrown when the index is out of bounds.</exception>
    public static T At<T>(
        this T[] value,
        int key,
        [CallerArgumentExpression(nameof(value))] string valueEx = "",
        [CallerArgumentExpression(nameof(key))] string keyEx = ""
    )
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        var total = value.Length;
        (0 <= key && key < total).IsTrue(
            $"{valueEx}[{key.WrapWithExpression(keyEx)}] is out of bounds [0,{total - 1}]"
        );

        return value[key];
    }

    /// <summary>
    /// Asserts that the array has the specified number of elements.
    /// </summary>
    /// <typeparam name="T">The type of elements in the array.</typeparam>
    /// <param name="value">The array to check.</param>
    /// <param name="count">The expected number of elements.</param>
    /// <param name="valueEx">The expression that produced the array.</param>
    /// <param name="countEx">The expression that produced the count.</param>
    /// <returns>The original array.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the array is null.</exception>
    /// <exception cref="AssertionFailedException">Thrown when the array length doesn't match the expected count.</exception>
    public static T[] Has<T>(
        this T[] value,
        int count,
        [CallerArgumentExpression(nameof(value))] string valueEx = "",
        [CallerArgumentExpression(nameof(count))] string countEx = ""
    )
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        var total = value.Length;
        total.Is(count, $"{valueEx} length `{total}` != `{count.WrapWithExpression(countEx)}`");

        return value;
    }

    /// <summary>
    /// Asserts that the array is empty.
    /// </summary>
    /// <typeparam name="T">The type of elements in the array.</typeparam>
    /// <param name="value">The array to check.</param>
    /// <param name="valueEx">The expression that produced the array.</param>
    /// <returns>The original array.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the array is null.</exception>
    /// <exception cref="AssertionFailedException">Thrown when the array is not empty.</exception>
    public static T[] IsEmpty<T>(this T[] value, [CallerArgumentExpression(nameof(value))] string valueEx = "")
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        var total = value.Length;
        total.Is(0, $"{valueEx} expected to be empty, but has `{total}` items");

        return value;
    }

    /// <summary>
    /// Asserts that the array is not empty.
    /// </summary>
    /// <typeparam name="T">The type of elements in the array.</typeparam>
    /// <param name="value">The array to check.</param>
    /// <param name="valueEx">The expression that produced the array.</param>
    /// <returns>The original array.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the array is null.</exception>
    /// <exception cref="AssertionFailedException">Thrown when the array is empty.</exception>
    public static T[] IsNotEmpty<T>(this T[] value, [CallerArgumentExpression(nameof(value))] string valueEx = "")
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        var total = value.Length;
        total.IsNot(0, $"{valueEx} expected to be not empty");

        return value;
    }
}
