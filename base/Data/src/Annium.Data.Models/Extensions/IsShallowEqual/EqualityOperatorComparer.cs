using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Annium.Data.Models.Extensions;

/// <summary>
/// Extension methods for shallow equality comparison - equality operator support.
/// </summary>
public static partial class IsShallowEqualExtensions
{
    /// <summary>
    /// Resolves the equality operator method for the specified type.
    /// </summary>
    /// <param name="type">The type to resolve the equality operator for.</param>
    /// <returns>The equality operator method if found, otherwise null.</returns>
    private static MethodInfo? ResolveEqualityOperatorMethod(Type type) =>
        type.GetMethods()
            .SingleOrDefault(x =>
                x is { IsSpecialName: true, Name: "op_Equality" } && x.GetParameters().All(y => y.ParameterType == type)
            );

    /// <summary>
    /// Builds a comparer expression that uses the type's equality operator.
    /// </summary>
    /// <param name="type">The type to build the comparer for.</param>
    /// <param name="equalityOperatorMethod">The equality operator method to use.</param>
    /// <returns>A lambda expression that uses the equality operator for comparison.</returns>
    private static LambdaExpression BuildEqualityOperatorComparer(Type type, MethodInfo equalityOperatorMethod)
    {
        var a = Expression.Parameter(type);
        var b = Expression.Parameter(type);

        return Expression.Lambda(Expression.Call(null, equalityOperatorMethod, a, b), a, b);
    }
}
