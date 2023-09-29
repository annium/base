using System.Runtime.CompilerServices;
using Annium.Testing.Assertions.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Testing;

public static class BooleanExtensions
{
    public static void IsTrue(
        this bool value,
        string? message = null,
        [CallerArgumentExpression("value")] string valueEx = default!
    )
    {
        if (!value)
            throw new AssertionFailedException(message ?? $"{value.Wrap(valueEx)} != True");
    }

    public static void IsFalse(
        this bool value,
        string? message = null,
        [CallerArgumentExpression("value")] string valueEx = default!
    )
    {
        if (value)
            throw new AssertionFailedException(message ?? $"{value.Wrap(valueEx)} != False");
    }
}