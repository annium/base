using System;
using System.Runtime.CompilerServices;

namespace Annium.Data.Operations.Extensions;

/// <summary>
/// Extension methods for status result types.
/// </summary>
public static class StatusResultExtensions
{
    /// <summary>
    /// Ensures that the status result has the specified status.
    /// </summary>
    /// <typeparam name="TS">The status type.</typeparam>
    /// <param name="result">The status result to check.</param>
    /// <param name="status">The expected status.</param>
    /// <param name="resultEx">The expression string for the result parameter.</param>
    /// <returns>The original result if it has the expected status.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the result does not have the expected status.</exception>
    public static IStatusResult<TS> EnsureHasStatus<TS>(
        this IStatusResult<TS> result,
        TS status,
        [CallerArgumentExpression(nameof(result))] string resultEx = ""
    )
    {
        if (result.Status is null || !result.Status.Equals(status))
            throw new InvalidOperationException($"Result {resultEx} has status {result.Status}, not {status}");

        return result;
    }

    /// <summary>
    /// Ensures that the status data result has the specified status.
    /// </summary>
    /// <typeparam name="TS">The status type.</typeparam>
    /// <typeparam name="TD">The data type.</typeparam>
    /// <param name="result">The status data result to check.</param>
    /// <param name="status">The expected status.</param>
    /// <param name="resultEx">The expression string for the result parameter.</param>
    /// <returns>The original result if it has the expected status.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the result does not have the expected status.</exception>
    public static IStatusResult<TS, TD> EnsureHasStatus<TS, TD>(
        this IStatusResult<TS, TD> result,
        TS status,
        [CallerArgumentExpression(nameof(result))] string resultEx = ""
    )
    {
        if (result.Status is null || !result.Status.Equals(status))
            throw new InvalidOperationException($"Result {resultEx} has status {result.Status}, not {status}");

        return result;
    }
}
