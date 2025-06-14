using System;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace Annium.Data.Operations;

/// <summary>
/// Extension methods for boolean results
/// </summary>
public static class BooleanResultExtensions
{
    /// <summary>
    /// Ensures that the boolean result represents success, throwing an exception if it doesn't
    /// </summary>
    /// <param name="result">The result to check</param>
    /// <param name="resultEx">The expression that generated the result</param>
    /// <returns>The same result if successful</returns>
    public static IBooleanResult EnsureSuccess(
        this IBooleanResult result,
        [CallerArgumentExpression(nameof(result))] string resultEx = ""
    )
    {
        if (result.IsFailure)
            throw new InvalidOperationException($"Result {resultEx} has failed");

        return result;
    }

    /// <summary>
    /// Ensures that the boolean result represents failure, throwing an exception if it doesn't
    /// </summary>
    /// <param name="result">The result to check</param>
    /// <param name="resultEx">The expression that generated the result</param>
    /// <returns>The same result if failed</returns>
    public static IBooleanResult EnsureFailure(
        this IBooleanResult result,
        [CallerArgumentExpression(nameof(result))] string resultEx = ""
    )
    {
        if (result.IsSuccess)
            throw new InvalidOperationException($"Result {resultEx} has succeed");

        return result;
    }

    /// <summary>
    /// Ensures that the boolean result with data represents success, throwing an exception if it doesn't
    /// </summary>
    /// <typeparam name="T">The type of the result data</typeparam>
    /// <param name="result">The result to check</param>
    /// <param name="resultEx">The expression that generated the result</param>
    /// <returns>The same result if successful</returns>
    public static IBooleanResult<T> EnsureSuccess<T>(
        this IBooleanResult<T> result,
        [CallerArgumentExpression(nameof(result))] string resultEx = ""
    )
    {
        if (result.IsFailure)
            throw new InvalidOperationException($"Result {resultEx} has failed");

        return result;
    }

    /// <summary>
    /// Ensures that the boolean result with data represents failure, throwing an exception if it doesn't
    /// </summary>
    /// <typeparam name="T">The type of the result data</typeparam>
    /// <param name="result">The result to check</param>
    /// <param name="resultEx">The expression that generated the result</param>
    /// <returns>The same result if failed</returns>
    public static IBooleanResult<T> EnsureFailure<T>(
        this IBooleanResult<T> result,
        [CallerArgumentExpression(nameof(result))] string resultEx = ""
    )
    {
        if (result.IsSuccess)
            throw new InvalidOperationException($"Result {resultEx} has succeed");

        return result;
    }
}
