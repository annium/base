using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace Annium.Testing;

/// <summary>
/// Provides extension methods for enumerable assertions in tests.
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// Asserts that the enumerable has the specified number of elements.
    /// </summary>
    /// <typeparam name="T">The type of elements in the enumerable.</typeparam>
    /// <param name="value">The enumerable to check.</param>
    /// <param name="count">The expected number of elements.</param>
    /// <param name="valueEx">The expression that produced the enumerable.</param>
    /// <param name="countEx">The expression that produced the count.</param>
    /// <returns>The original enumerable.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the enumerable is null.</exception>
    /// <exception cref="AssertionFailedException">Thrown when the enumerable count doesn't match the expected count.</exception>
    public static IEnumerable<T> Has<T>(
        this IEnumerable<T> value,
        int count,
        [CallerArgumentExpression(nameof(value))] string valueEx = default!,
        [CallerArgumentExpression(nameof(count))] string countEx = default!
    )
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        var total = value.Count();
        total.Is(count, $"{valueEx} count `{total}` != `{count.WrapWithExpression(countEx)}`");

        return value;
    }

    /// <summary>
    /// Asserts that the enumerable is empty.
    /// </summary>
    /// <typeparam name="T">The type of elements in the enumerable.</typeparam>
    /// <param name="value">The enumerable to check.</param>
    /// <param name="valueEx">The expression that produced the enumerable.</param>
    /// <returns>The original enumerable.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the enumerable is null.</exception>
    /// <exception cref="AssertionFailedException">Thrown when the enumerable is not empty.</exception>
    public static IEnumerable<T> IsEmpty<T>(
        this IEnumerable<T> value,
        [CallerArgumentExpression(nameof(value))] string valueEx = default!
    )
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        var total = value.Count();
        total.Is(0, $"{valueEx} expected to be empty, but has `{total}` items");

        return value;
    }

    /// <summary>
    /// Asserts that the enumerable is not empty.
    /// </summary>
    /// <typeparam name="T">The type of elements in the enumerable.</typeparam>
    /// <param name="value">The enumerable to check.</param>
    /// <param name="valueEx">The expression that produced the enumerable.</param>
    /// <returns>The original enumerable.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the enumerable is null.</exception>
    /// <exception cref="AssertionFailedException">Thrown when the enumerable is empty.</exception>
    public static IEnumerable<T> IsNotEmpty<T>(
        this IEnumerable<T> value,
        [CallerArgumentExpression(nameof(value))] string valueEx = default!
    )
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        var total = value.Count();
        total.IsNot(0, $"{valueEx} expected to be not empty");

        return value;
    }

    /// <summary>
    /// Asserts that the enumerable has an element at the specified index and returns it.
    /// </summary>
    /// <typeparam name="T">The type of elements in the enumerable.</typeparam>
    /// <param name="value">The enumerable to check.</param>
    /// <param name="key">The index to check.</param>
    /// <param name="valueEx">The expression that produced the enumerable.</param>
    /// <param name="keyEx">The expression that produced the index.</param>
    /// <returns>The element at the specified index.</returns>
    /// <exception cref="AssertionFailedException">Thrown when the index is out of bounds.</exception>
    public static T At<T>(
        this IEnumerable<T> value,
        int key,
        [CallerArgumentExpression(nameof(value))] string valueEx = default!,
        [CallerArgumentExpression(nameof(key))] string keyEx = default!
    )
    {
        var val = value.ToArray();
        (0 <= key && key < val.Length).IsTrue(
            $"{valueEx}[{key.WrapWithExpression(keyEx)}] is out of bounds [0,{val.Length - 1}]"
        );
        return val[key];
    }
}
