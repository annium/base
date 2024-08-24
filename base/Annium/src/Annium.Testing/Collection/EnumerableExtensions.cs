using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace Annium.Testing;

public static class EnumerableExtensions
{
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

    public static IEnumerable<T> Has<T>(
        this IEnumerable<T> value,
        int count,
        [CallerArgumentExpression(nameof(value))] string valueEx = default!,
        [CallerArgumentExpression(nameof(count))] string countEx = default!
    )
    {
        var val = value.ToArray();
        val.Length.Is(count, $"{valueEx} count `{val.Length}` != `{count.WrapWithExpression(countEx)}`");

        return val;
    }

    public static IEnumerable<T> IsEmpty<T>(
        this IEnumerable<T> value,
        [CallerArgumentExpression(nameof(value))] string valueEx = default!
    )
    {
        var val = value.ToArray();
        val.Length.Is(0, $"{valueEx} expected to be empty, but has `{val.Length}` items");

        return val;
    }

    public static IEnumerable<T> IsNotEmpty<T>(
        this IEnumerable<T> value,
        [CallerArgumentExpression(nameof(value))] string valueEx = default!
    )
    {
        var val = value.ToArray();
        val.Length.IsGreater(0, $"{valueEx} expected to be not empty");

        return val;
    }
}
