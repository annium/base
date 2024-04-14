using System;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace Annium.Data.Operations;

public static class ResultBaseExtensions
{
    public static void ThrowIfHasErrors(
        this IResultBase result,
        [CallerArgumentExpression("result")] string resultEx = default!
    )
    {
        if (result.HasErrors)
            throw new InvalidOperationException(
                $"Result {resultEx} has errors: {Environment.NewLine}{result.ErrorState()}"
            );
    }
}
