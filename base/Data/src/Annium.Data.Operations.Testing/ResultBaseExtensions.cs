using System;
using System.Runtime.CompilerServices;
using Annium.Testing;

namespace Annium.Data.Operations.Testing;

/// <summary>
/// Testing extensions for result base types
/// </summary>
public static class ResultBaseExtensions
{
    /// <summary>
    /// Asserts that the result has no errors
    /// </summary>
    /// <param name="value">The result to check</param>
    /// <param name="message">Optional custom error message</param>
    /// <param name="valueEx">The expression that generated the value</param>
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

    /// <summary>
    /// Asserts that the result has errors
    /// </summary>
    /// <param name="value">The result to check</param>
    /// <param name="message">Optional custom error message</param>
    /// <param name="valueEx">The expression that generated the value</param>
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
