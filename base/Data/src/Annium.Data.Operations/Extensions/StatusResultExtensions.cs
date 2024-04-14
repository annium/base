using System;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace Annium.Data.Operations;

public static class StatusResultExtensions
{
    public static IStatusResult<TS> EnsureHasStatus<TS>(
        this IStatusResult<TS> result,
        TS status,
        [CallerArgumentExpression("result")] string resultEx = default!
    )
    {
        if (result.Status is null || !result.Status.Equals(status))
            throw new InvalidOperationException($"Result {resultEx} has status {result.Status}, not {status}");

        return result;
    }

    public static IStatusResult<TS, TD> EnsureHasStatus<TS, TD>(
        this IStatusResult<TS, TD> result,
        TS status,
        [CallerArgumentExpression("result")] string resultEx = default!
    )
    {
        if (result.Status is null || !result.Status.Equals(status))
            throw new InvalidOperationException($"Result {resultEx} has status {result.Status}, not {status}");

        return result;
    }
}
