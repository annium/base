using System.Runtime.CompilerServices;

namespace Annium.Testing;

public static class TypeExtensions
{
    public static T As<T>(
        this object? value,
        string? message = null,
        [CallerArgumentExpression("value")] string valueEx = default!
    )
    {
        (value is T).IsTrue(message ?? $"{valueEx} is {value?.GetType()}, not {typeof(T)}");

        return (T)value!;
    }

    public static T AsExact<T>(
        this object? value,
        string? message = null,
        [CallerArgumentExpression("value")] string valueEx = default!
    )
    {
        (value?.GetType() == typeof(T)).IsTrue(message ?? $"{valueEx} is {value?.GetType()}, not {typeof(T)}");

        return (T)value!;
    }
}