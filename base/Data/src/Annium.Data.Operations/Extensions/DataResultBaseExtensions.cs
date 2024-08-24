using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace Annium.Data.Operations;

public static class DataResultBaseExtensions
{
    public static T Unwrap<T>(
        this IDataResultBase<T> result,
        [CallerArgumentExpression(nameof(result))] string resultEx = default!
    )
    {
        result.ThrowIfHasErrors(resultEx);

        return result.Data;
    }
}
