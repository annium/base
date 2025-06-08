using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace Annium.Testing;

/// <summary>
/// Provides extension methods for dictionary assertions in tests.
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    /// Asserts that the dictionary contains the specified key and returns its value.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="value">The dictionary to check.</param>
    /// <param name="key">The key to check for.</param>
    /// <param name="valueEx">The expression that produced the dictionary.</param>
    /// <param name="keyEx">The expression that produced the key.</param>
    /// <returns>The value associated with the specified key.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the dictionary is null.</exception>
    /// <exception cref="AssertionFailedException">Thrown when the key is not found in the dictionary.</exception>
    public static TValue At<TKey, TValue>(
        this IDictionary<TKey, TValue> value,
        TKey key,
        [CallerArgumentExpression(nameof(value))] string valueEx = default!,
        [CallerArgumentExpression(nameof(key))] string keyEx = default!
    )
        where TKey : notnull
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        value.ContainsKey(key).IsTrue($"{valueEx} has no key `{key.WrapWithExpression(keyEx)}`");

        return value[key];
    }

    /// <summary>
    /// Asserts that the read-only dictionary contains the specified key and returns its value.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="value">The dictionary to check.</param>
    /// <param name="key">The key to check for.</param>
    /// <param name="valueEx">The expression that produced the dictionary.</param>
    /// <param name="keyEx">The expression that produced the key.</param>
    /// <returns>The value associated with the specified key.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the dictionary is null.</exception>
    /// <exception cref="AssertionFailedException">Thrown when the key is not found in the dictionary.</exception>
    public static TValue At<TKey, TValue>(
        this IReadOnlyDictionary<TKey, TValue> value,
        TKey key,
        [CallerArgumentExpression(nameof(value))] string valueEx = default!,
        [CallerArgumentExpression(nameof(key))] string keyEx = default!
    )
        where TKey : notnull
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        value.ContainsKey(key).IsTrue($"{valueEx} has no key `{key.WrapWithExpression(keyEx)}`");

        return value[key];
    }

    /// <summary>
    /// Asserts that the dictionary has the specified number of elements.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="value">The dictionary to check.</param>
    /// <param name="count">The expected number of elements.</param>
    /// <param name="valueEx">The expression that produced the dictionary.</param>
    /// <param name="countEx">The expression that produced the count.</param>
    /// <returns>The original dictionary.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the dictionary is null.</exception>
    /// <exception cref="AssertionFailedException">Thrown when the dictionary count doesn't match the expected count.</exception>
    public static IDictionary<TKey, TValue> Has<TKey, TValue>(
        this IDictionary<TKey, TValue> value,
        int count,
        [CallerArgumentExpression(nameof(value))] string valueEx = default!,
        [CallerArgumentExpression(nameof(count))] string countEx = default!
    )
        where TKey : notnull
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        var total = value.Count;
        total.Is(count, $"{valueEx} count `{total}` != `{count.WrapWithExpression(countEx)}`");

        return value;
    }

    /// <summary>
    /// Asserts that the read-only dictionary has the specified number of elements.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="value">The dictionary to check.</param>
    /// <param name="count">The expected number of elements.</param>
    /// <param name="valueEx">The expression that produced the dictionary.</param>
    /// <param name="countEx">The expression that produced the count.</param>
    /// <returns>The original dictionary.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the dictionary is null.</exception>
    /// <exception cref="AssertionFailedException">Thrown when the dictionary count doesn't match the expected count.</exception>
    public static IReadOnlyDictionary<TKey, TValue> Has<TKey, TValue>(
        this IReadOnlyDictionary<TKey, TValue> value,
        int count,
        [CallerArgumentExpression(nameof(value))] string valueEx = default!,
        [CallerArgumentExpression(nameof(count))] string countEx = default!
    )
        where TKey : notnull
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        var total = value.Count;
        total.Is(count, $"{valueEx} count `{total}` != `{count.WrapWithExpression(countEx)}`");

        return value;
    }

    /// <summary>
    /// Asserts that the dictionary is empty.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="value">The dictionary to check.</param>
    /// <param name="valueEx">The expression that produced the dictionary.</param>
    /// <returns>The original dictionary.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the dictionary is null.</exception>
    /// <exception cref="AssertionFailedException">Thrown when the dictionary is not empty.</exception>
    public static IDictionary<TKey, TValue> IsEmpty<TKey, TValue>(
        this IDictionary<TKey, TValue> value,
        [CallerArgumentExpression(nameof(value))] string valueEx = default!
    )
        where TKey : notnull
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        var total = value.Count;
        total.Is(0, $"{valueEx} expected to be empty, but has `{total}` items");

        return value;
    }

    /// <summary>
    /// Asserts that the read-only dictionary is empty.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="value">The dictionary to check.</param>
    /// <param name="valueEx">The expression that produced the dictionary.</param>
    /// <returns>The original dictionary.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the dictionary is null.</exception>
    /// <exception cref="AssertionFailedException">Thrown when the dictionary is not empty.</exception>
    public static IReadOnlyDictionary<TKey, TValue> IsEmpty<TKey, TValue>(
        this IReadOnlyDictionary<TKey, TValue> value,
        [CallerArgumentExpression(nameof(value))] string valueEx = default!
    )
        where TKey : notnull
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        var total = value.Count;
        total.Is(0, $"{valueEx} expected to be empty, but has `{total}` items");

        return value;
    }

    /// <summary>
    /// Asserts that the dictionary is not empty.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="value">The dictionary to check.</param>
    /// <param name="valueEx">The expression that produced the dictionary.</param>
    /// <returns>The original dictionary.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the dictionary is null.</exception>
    /// <exception cref="AssertionFailedException">Thrown when the dictionary is empty.</exception>
    public static IDictionary<TKey, TValue> IsNotEmpty<TKey, TValue>(
        this IDictionary<TKey, TValue> value,
        [CallerArgumentExpression(nameof(value))] string valueEx = default!
    )
        where TKey : notnull
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        var total = value.Count;
        total.IsNot(0, $"{valueEx} expected to be not empty");

        return value;
    }

    /// <summary>
    /// Asserts that the read-only dictionary is not empty.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="value">The dictionary to check.</param>
    /// <param name="valueEx">The expression that produced the dictionary.</param>
    /// <returns>The original dictionary.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the dictionary is null.</exception>
    /// <exception cref="AssertionFailedException">Thrown when the dictionary is empty.</exception>
    public static IReadOnlyDictionary<TKey, TValue> IsNotEmpty<TKey, TValue>(
        this IReadOnlyDictionary<TKey, TValue> value,
        [CallerArgumentExpression(nameof(value))] string valueEx = default!
    )
        where TKey : notnull
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        var total = value.Count;
        total.IsNot(0, $"{valueEx} expected to be not empty");

        return value;
    }
}
