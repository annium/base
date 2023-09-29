using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Annium.Testing.Assertions.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Testing;

public static class DictionaryExtensions
{
    public static TValue At<TKey, TValue>(
        this IDictionary<TKey, TValue> value,
        TKey key,
        [CallerArgumentExpression("value")] string valueEx = default!,
        [CallerArgumentExpression("key")] string keyEx = default!
    )
        where TKey : notnull
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        value.ContainsKey(key).IsTrue($"{valueEx} has no key `{key.Wrap(keyEx)}`");

        return value[key];
    }

    public static TValue At<TKey, TValue>(
        this IReadOnlyDictionary<TKey, TValue> value,
        TKey key,
        [CallerArgumentExpression("value")] string valueEx = default!,
        [CallerArgumentExpression("key")] string keyEx = default!
    )
        where TKey : notnull
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        value.ContainsKey(key).IsTrue($"{valueEx} has no key `{key.Wrap(keyEx)}`");

        return value[key];
    }

    public static IDictionary<TKey, TValue> Has<TKey, TValue>(
        this IDictionary<TKey, TValue> value,
        int count,
        [CallerArgumentExpression("value")] string valueEx = default!,
        [CallerArgumentExpression("count")] string countEx = default!
    )
        where TKey : notnull
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        var total = value.Count;
        total.Is(count, $"{valueEx} count `{total}` != `{count.Wrap(countEx)}`");

        return value;
    }

    public static IReadOnlyDictionary<TKey, TValue> Has<TKey, TValue>(
        this IReadOnlyDictionary<TKey, TValue> value,
        int count,
        [CallerArgumentExpression("value")] string valueEx = default!,
        [CallerArgumentExpression("count")] string countEx = default!
    )
        where TKey : notnull
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        var total = value.Count;
        total.Is(count, $"{valueEx} count `{total}` != `{count.Wrap(countEx)}`");

        return value;
    }

    public static IDictionary<TKey, TValue> IsEmpty<TKey, TValue>(
        this IDictionary<TKey, TValue> value,
        [CallerArgumentExpression("value")] string valueEx = default!
    )
        where TKey : notnull
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        var total = value.Count;
        total.Is(0, $"{valueEx} expected to be empty, but has `{total}` items");

        return value;
    }

    public static IReadOnlyDictionary<TKey, TValue> IsEmpty<TKey, TValue>(
        this IReadOnlyDictionary<TKey, TValue> value,
        [CallerArgumentExpression("value")] string valueEx = default!
    )
        where TKey : notnull
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        var total = value.Count;
        total.Is(0, $"{valueEx} expected to be empty, but has `{total}` items");

        return value;
    }

    public static IDictionary<TKey, TValue> IsNotEmpty<TKey, TValue>(
        this IDictionary<TKey, TValue> value,
        [CallerArgumentExpression("value")] string valueEx = default!
    )
        where TKey : notnull
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        var total = value.Count;
        total.IsNot(0, $"{valueEx} expected to be not empty");

        return value;
    }

    public static IReadOnlyDictionary<TKey, TValue> IsNotEmpty<TKey, TValue>(
        this IReadOnlyDictionary<TKey, TValue> value,
        [CallerArgumentExpression("value")] string valueEx = default!
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