using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Annium.Testing;

public static class ValueExtensions
{
    public static T IsDefault<T>(
        this T value,
        string? message = null,
        [CallerArgumentExpression(nameof(value))] string valueEx = default!
    )
    {
        value.Is(default, $"{value.WrapWithExpression(valueEx)} is not default");

        return value;
    }

    [return: NotNull]
    public static T IsNotDefault<T>(
        [NotNull] this T value,
        string? message = null,
        [CallerArgumentExpression(nameof(value))] string valueEx = default!
    )
    {
        value.IsNot(default, $"{value.WrapWithExpression(valueEx)} is default");
        ArgumentNullException.ThrowIfNull(value);

        return value;
    }
}
