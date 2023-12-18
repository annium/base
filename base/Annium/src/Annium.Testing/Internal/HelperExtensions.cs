using System;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Annium.Testing.Internal;

internal static class HelperExtensions
{
    public static string Wrap<T>(this T value, string ex)
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
