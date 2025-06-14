using System;
using System.Runtime.CompilerServices;

namespace Annium.Data.Operations.Extensions;

/// <summary>
/// Extension methods for result base types
/// </summary>
public static class ResultBaseExtensions
{
    /// <summary>
    /// Throws an exception if the result has any errors
    /// </summary>
    /// <param name="result">The result to check</param>
    /// <param name="resultEx">The expression that generated the result</param>
    public static void ThrowIfHasErrors(
        this IResultBase result,
        [CallerArgumentExpression(nameof(result))] string resultEx = ""
    )
    {
        if (result.HasErrors)
            throw new InvalidOperationException(
                $"Result {resultEx} has errors: {Environment.NewLine}{result.ErrorState()}"
            );
    }
}
