using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;

namespace Annium.Testing;

public static class TestValueExtensions
{
    public static string WrapWithExpression<T>(this T value, string ex)
    {
        var v = value.Stringify();

        return v == ex ? v : $"{ex} ({v})";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string Stringify<T>(this T value)
    {
        if (typeof(T) == typeof(Type))
            return typeof(T).FriendlyName();

        if (value is IEnumerable enumerable)
            return Stringify(enumerable);

        return value?.ToString() ?? "null";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string Stringify(this IEnumerable value)
    {
        var enumerator = value.GetEnumerator();
        try
        {
            if (!enumerator.MoveNext())
                return "[]";

            var sb = new StringBuilder("[");
            sb.Append(enumerator.Current);

            while (enumerator.MoveNext())
            {
                sb.Append(", ");
                sb.Append(enumerator.Current);
            }

            sb.Append("]");

            return sb.ToString();
        }
        finally
        {
            if (enumerator is IDisposable disposable)
                disposable.Dispose();
        }
    }
}
