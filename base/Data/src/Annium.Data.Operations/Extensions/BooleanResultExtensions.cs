using System;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace Annium.Data.Operations;

public static class BooleanResultExtensions
{
    public static IBooleanResult EnsureSuccess(
        this IBooleanResult result,
        [CallerArgumentExpression(nameof(result))] string resultEx = default!
    )
    {
        if (result.IsFailure)
            throw new InvalidOperationException($"Result {resultEx} has failed");

        return result;
    }

    public static IBooleanResult EnsureFailure(
        this IBooleanResult result,
        [CallerArgumentExpression(nameof(result))] string resultEx = default!
    )
    {
        if (result.IsSuccess)
            throw new InvalidOperationException($"Result {resultEx} has succeed");

        return result;
    }

    public static IBooleanResult<T> EnsureSuccess<T>(
        this IBooleanResult<T> result,
        [CallerArgumentExpression(nameof(result))] string resultEx = default!
    )
    {
        if (result.IsFailure)
            throw new InvalidOperationException($"Result {resultEx} has failed");

        return result;
    }

    public static IBooleanResult<T> EnsureFailure<T>(
        this IBooleanResult<T> result,
        [CallerArgumentExpression(nameof(result))] string resultEx = default!
    )
    {
        if (result.IsSuccess)
            throw new InvalidOperationException($"Result {resultEx} has succeed");

        return result;
    }
}
