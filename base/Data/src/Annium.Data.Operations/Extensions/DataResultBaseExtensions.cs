using System.Runtime.CompilerServices;

namespace Annium.Data.Operations.Extensions;

/// <summary>
/// Extension methods for data result base types
/// </summary>
public static class DataResultBaseExtensions
{
    /// <summary>
    /// Unwraps the data from a result, throwing an exception if the result has errors
    /// </summary>
    /// <typeparam name="T">The type of the result data</typeparam>
    /// <param name="result">The result to unwrap</param>
    /// <param name="resultEx">The expression that generated the result</param>
    /// <returns>The data from the result</returns>
    public static T Unwrap<T>(
        this IDataResultBase<T> result,
        [CallerArgumentExpression(nameof(result))] string resultEx = default!
    )
    {
        result.ThrowIfHasErrors(resultEx);

        return result.Data;
    }
}
