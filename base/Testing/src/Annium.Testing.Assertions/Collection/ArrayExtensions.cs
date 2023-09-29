using System;
using System.Runtime.CompilerServices;
using Annium.Testing.Assertions.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Testing;

public static class ArrayExtensions
{
    public static T At<T>(
        this T[] value,
        int key,
        [CallerArgumentExpression("value")] string valueEx = default!,
        [CallerArgumentExpression("key")] string keyEx = default!
    )
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        var total = value.Length;
        (0 <= key && key < total).IsTrue($"{valueEx}[{key.Wrap(keyEx)}] is out of bounds [0,{total - 1}]");

        return value[key];
    }

    public static T[] Has<T>(
        this T[] value,
        int count,
        [CallerArgumentExpression("value")] string valueEx = default!,
        [CallerArgumentExpression("count")] string countEx = default!
    )
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        var total = value.Length;
        total.Is(count, $"{valueEx} length `{total}` != `{count.Wrap(countEx)}`");

        return value;
    }

    public static T[] IsEmpty<T>(
        this T[] value,
        [CallerArgumentExpression("value")] string valueEx = default!
    )
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        var total = value.Length;
        total.Is(0, $"{valueEx} expected to be empty, but has `{total}` items");

        return value;
    }

    public static T[] IsNotEmpty<T>(
        this T[] value,
        [CallerArgumentExpression("value")] string valueEx = default!
    )
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        var total = value.Length;
        total.IsNot(0, $"{valueEx} expected to be not empty");

        return value;
    }
}