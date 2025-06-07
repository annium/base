using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;

namespace Annium.Testing;

/// <summary>
/// Provides extension methods for test value formatting and expression wrapping.
/// </summary>
public static class TestValueExtensions
{
    /// <summary>
    /// Wraps a value with its expression for display in assertion messages.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <param name="ex">The expression that produced the value.</param>
    /// <returns>A string combining the expression and the value.</returns>
    public static string WrapWithExpression<T>(this T value, string ex)
    {
        var v = value.Stringify();

        return v == ex ? v : $"{ex} ({v})";
    }

    /// <summary>
    /// Converts a value to its string representation, handling special cases for types and enumerables.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to stringify.</param>
    /// <returns>The string representation of the value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string Stringify<T>(this T value)
    {
        if (typeof(T) == typeof(Type))
            return typeof(T).FriendlyName();

        if (value is IEnumerable enumerable)
            return Stringify(enumerable);

        return value?.ToString() ?? "null";
    }

    /// <summary>
    /// Converts an enumerable to its string representation.
    /// </summary>
    /// <param name="value">The enumerable to stringify.</param>
    /// <returns>The string representation of the enumerable.</returns>
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
