using System;
using System.Runtime.CompilerServices;
using Annium.Testing;

namespace Annium.Data.Operations.Testing;

public static class ResultBaseExtensions
{
    public static void HasNoErrors(
        this IResultBase value,
        string? message = null,
        [CallerArgumentExpression(nameof(value))] string valueEx = default!
    )
    {
        if (value.HasErrors)
            throw new AssertionFailedException(
                message
                    ?? $"{value.WrapWithExpression(valueEx)} contains errors: {Environment.NewLine}{value.ErrorState()}"
            );
    }

    public static void HasErrors(
        this IResultBase value,
        string? message = null,
        [CallerArgumentExpression(nameof(value))] string valueEx = default!
    )
    {
        if (value.IsOk)
            throw new AssertionFailedException(message ?? $"{value.WrapWithExpression(valueEx)} contains no errors");
    }
}
