using System.Runtime.CompilerServices;

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
            throw new AssertionFailedException(message ?? $"{value.WrapWithExpression(valueEx)} != True");
    }

    public static void IsFalse(
        this bool value,
        string? message = null,
        [CallerArgumentExpression("value")] string valueEx = default!
    )
    {
        if (value)
            throw new AssertionFailedException(message ?? $"{value.WrapWithExpression(valueEx)} != False");
    }
}
