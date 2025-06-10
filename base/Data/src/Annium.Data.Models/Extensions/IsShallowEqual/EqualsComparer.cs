using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Annium.Data.Models.Extensions.IsShallowEqual;

/// <summary>
/// Extension methods for shallow equality comparison - Equals method support.
/// </summary>
public static partial class IsShallowEqualExtensions
{
    /// <summary>
    /// Resolves the Equals method for the specified type.
    /// </summary>
    /// <param name="type">The type to resolve the Equals method for.</param>
    /// <returns>The Equals method if found, otherwise null.</returns>
    private static MethodInfo? ResolveEqualsMethod(Type type)
    {
        var methods = type.GetMethods()
            .Where(x => x is { IsPublic: true, IsStatic: false, Name: nameof(Equals) } && x.GetParameters().Length == 1)
            .ToArray();

        var equals = methods.SingleOrDefault(x => x.GetParameters()[0].ParameterType == type);
        if (equals != null)
            return equals;

        return methods.SingleOrDefault(x =>
            x.DeclaringType == type && x.GetParameters()[0].ParameterType == typeof(object)
        );
    }

    /// <summary>
    /// Builds a comparer expression that uses the type's Equals method.
    /// </summary>
    /// <param name="type">The type to build the comparer for.</param>
    /// <param name="equalsMethod">The Equals method to use.</param>
    /// <returns>A lambda expression that uses the Equals method for comparison.</returns>
    private static LambdaExpression BuildEqualsComparer(Type type, MethodInfo equalsMethod)
    {
        var a = Expression.Parameter(type);
        var b = Expression.Parameter(type);
        var parameters = new List<ParameterExpression> { a, b };

        var returnTarget = Expression.Label(typeof(bool));

        var expressions = new List<Expression>();

        if (type.IsClass)
            expressions.AddRange(AddReferenceEqualityChecks(a, b, returnTarget));

        var equalsExpression =
            equalsMethod.GetParameters()[0].ParameterType == type
                ? Expression.Call(a, equalsMethod, b)
                : Expression.Call(
                    a,
                    equalsMethod,
                    Expression.Convert(b, equalsMethod.GetParameters()[0].ParameterType)
                );

        expressions.Add(Expression.Label(returnTarget, equalsExpression));

        return Expression.Lambda(Expression.Block(new List<ParameterExpression>(), expressions), parameters);
    }
}
