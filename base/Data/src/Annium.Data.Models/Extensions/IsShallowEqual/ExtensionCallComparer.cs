using System;
using System.Linq;
using System.Linq.Expressions;
using Annium.Core.Mapper;

namespace Annium.Data.Models.Extensions.IsShallowEqual;

/// <summary>
/// Extension methods for shallow equality comparison - recursive call support.
/// </summary>
public static partial class IsShallowEqualExtensions
{
    /// <summary>
    /// Builds a comparer expression that makes a recursive call to the extension method.
    /// </summary>
    /// <param name="type">The type to build the comparer for.</param>
    /// <param name="mapper">The mapper to use for type conversions.</param>
    /// <returns>A lambda expression that calls the extension method recursively.</returns>
    private static LambdaExpression BuildExtensionCallComparer(Type type, IMapper mapper)
    {
        var a = Expression.Parameter(type);
        var b = Expression.Parameter(type);
        var m = Expression.Constant(mapper);

        var method = typeof(IsShallowEqualExtensions)
            .GetMethods()
            .Single(x => x.GetParameters().Length == 3)
            .MakeGenericMethod(type, type);

        return Expression.Lambda(Expression.Call(null, method, a, b, m), a, b);
    }
}
