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
        [CallerArgumentExpression(nameof(value))] string valueEx = "",
        [CallerArgumentExpression(nameof(key))] string keyEx = ""
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
        [CallerArgumentExpression(nameof(value))] string valueEx = "",
        [CallerArgumentExpression(nameof(key))] string keyEx = ""
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
    /// <param name="message">The message to include in the exception if the assertion fails.</param>
    /// <param name="valueEx">The expression that produced the dictionary.</param>
    /// <param name="countEx">The expression that produced the count.</param>
    /// <returns>The original dictionary.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the dictionary is null.</exception>
    /// <exception cref="AssertionFailedException">Thrown when the dictionary count doesn't match the expected count.</exception>
    public static IDictionary<TKey, TValue> Has<TKey, TValue>(
        this IDictionary<TKey, TValue> value,
        int count,
        string? message = null,
        [CallerArgumentExpression(nameof(value))] string valueEx = "",
        [CallerArgumentExpression(nameof(count))] string countEx = ""
    )
        where TKey : notnull
    {
        CheckCount(value, value?.Count ?? 0, count, message, valueEx, countEx);
        return value!;
    }

    /// <summary>
    /// Asserts that the read-only dictionary has the specified number of elements.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="value">The dictionary to check.</param>
    /// <param name="count">The expected number of elements.</param>
    /// <param name="message">The message to include in the exception if the assertion fails.</param>
    /// <param name="valueEx">The expression that produced the dictionary.</param>
    /// <param name="countEx">The expression that produced the count.</param>
    /// <returns>The original dictionary.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the dictionary is null.</exception>
    /// <exception cref="AssertionFailedException">Thrown when the dictionary count doesn't match the expected count.</exception>
    public static IReadOnlyDictionary<TKey, TValue> Has<TKey, TValue>(
        this IReadOnlyDictionary<TKey, TValue> value,
        int count,
        string? message = null,
        [CallerArgumentExpression(nameof(value))] string valueEx = "",
        [CallerArgumentExpression(nameof(count))] string countEx = ""
    )
        where TKey : notnull
    {
        CheckCount(value, value?.Count ?? 0, count, message, valueEx, countEx);
        return value!;
    }

    /// <summary>
    /// Asserts that the dictionary is empty.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="value">The dictionary to check.</param>
    /// <param name="message">The message to include in the exception if the assertion fails.</param>
    /// <param name="valueEx">The expression that produced the dictionary.</param>
    /// <returns>The original dictionary.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the dictionary is null.</exception>
    /// <exception cref="AssertionFailedException">Thrown when the dictionary is not empty.</exception>
    public static IDictionary<TKey, TValue> IsEmpty<TKey, TValue>(
        this IDictionary<TKey, TValue> value,
        string? message = null,
        [CallerArgumentExpression(nameof(value))] string valueEx = ""
    )
        where TKey : notnull
    {
        CheckEmpty(value, value?.Count ?? 0, message, valueEx);
        return value!;
    }

    /// <summary>
    /// Asserts that the read-only dictionary is empty.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="value">The dictionary to check.</param>
    /// <param name="message">The message to include in the exception if the assertion fails.</param>
    /// <param name="valueEx">The expression that produced the dictionary.</param>
    /// <returns>The original dictionary.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the dictionary is null.</exception>
    /// <exception cref="AssertionFailedException">Thrown when the dictionary is not empty.</exception>
    public static IReadOnlyDictionary<TKey, TValue> IsEmpty<TKey, TValue>(
        this IReadOnlyDictionary<TKey, TValue> value,
        string? message = null,
        [CallerArgumentExpression(nameof(value))] string valueEx = ""
    )
        where TKey : notnull
    {
        CheckEmpty(value, value?.Count ?? 0, message, valueEx);
        return value!;
    }

    /// <summary>
    /// Asserts that the dictionary is not empty.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="value">The dictionary to check.</param>
    /// <param name="message">The message to include in the exception if the assertion fails.</param>
    /// <param name="valueEx">The expression that produced the dictionary.</param>
    /// <returns>The original dictionary.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the dictionary is null.</exception>
    /// <exception cref="AssertionFailedException">Thrown when the dictionary is empty.</exception>
    public static IDictionary<TKey, TValue> IsNotEmpty<TKey, TValue>(
        this IDictionary<TKey, TValue> value,
        string? message = null,
        [CallerArgumentExpression(nameof(value))] string valueEx = ""
    )
        where TKey : notnull
    {
        CheckNotEmpty(value, value?.Count ?? 0, message, valueEx);
        return value!;
    }

    /// <summary>
    /// Asserts that the read-only dictionary is not empty.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <param name="value">The dictionary to check.</param>
    /// <param name="message">The message to include in the exception if the assertion fails.</param>
    /// <param name="valueEx">The expression that produced the dictionary.</param>
    /// <returns>The original dictionary.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the dictionary is null.</exception>
    /// <exception cref="AssertionFailedException">Thrown when the dictionary is empty.</exception>
    public static IReadOnlyDictionary<TKey, TValue> IsNotEmpty<TKey, TValue>(
        this IReadOnlyDictionary<TKey, TValue> value,
        string? message = null,
        [CallerArgumentExpression(nameof(value))] string valueEx = ""
    )
        where TKey : notnull
    {
        CheckNotEmpty(value, value?.Count ?? 0, message, valueEx);
        return value!;
    }

    /// <summary>Asserts that <paramref name="value"/> is non-null and its count equals <paramref name="count"/>.</summary>
    /// <param name="value">The dictionary to validate.</param>
    /// <param name="actualCount">The dictionary's actual count.</param>
    /// <param name="count">The expected count.</param>
    /// <param name="message">The message to include in the exception if the assertion fails.</param>
    /// <param name="valueEx">The expression that produced the dictionary.</param>
    /// <param name="countEx">The expression that produced the expected count.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    private static void CheckCount(
        object? value,
        int actualCount,
        int count,
        string? message,
        string valueEx,
        string countEx
    )
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        actualCount.Is(count, message ?? $"{valueEx} count `{actualCount}` != `{count.WrapWithExpression(countEx)}`");
    }

    /// <summary>Asserts that <paramref name="value"/> is non-null and its count is zero.</summary>
    /// <param name="value">The dictionary to validate.</param>
    /// <param name="actualCount">The dictionary's actual count.</param>
    /// <param name="message">The message to include in the exception if the assertion fails.</param>
    /// <param name="valueEx">The expression that produced the dictionary.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    private static void CheckEmpty(object? value, int actualCount, string? message, string valueEx)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        actualCount.Is(0, message ?? $"{valueEx} expected to be empty, but has `{actualCount}` items");
    }

    /// <summary>Asserts that <paramref name="value"/> is non-null and its count is non-zero.</summary>
    /// <param name="value">The dictionary to validate.</param>
    /// <param name="actualCount">The dictionary's actual count.</param>
    /// <param name="message">The message to include in the exception if the assertion fails.</param>
    /// <param name="valueEx">The expression that produced the dictionary.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    private static void CheckNotEmpty(object? value, int actualCount, string? message, string valueEx)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        actualCount.IsNot(0, message ?? $"{valueEx} expected to be not empty");
    }
}
