using System;
using System.Runtime.CompilerServices;

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

        return value?.ToString() ?? "null";
    }
}
