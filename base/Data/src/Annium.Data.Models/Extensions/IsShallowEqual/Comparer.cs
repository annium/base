using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Annium.Core.Mapper;

namespace Annium.Data.Models.Extensions.IsShallowEqual;

/// <summary>
/// Extension methods for shallow equality comparison
/// </summary>
public static partial class IsShallowEqualExtensions
{
    /// <summary>
    /// Builds a comparer expression for the specified type.
    /// </summary>
    /// <param name="type">The type to build a comparer for.</param>
    /// <param name="mapper">The mapper to use for type conversions.</param>
    /// <returns>A lambda expression that can compare two instances of the specified type.</returns>
    private static LambdaExpression BuildComparer(Type type, IMapper mapper)
    {
        // if Equality operator is overriden - return it's call
        var equalityOperatorMethod = ResolveEqualityOperatorMethod(type);
        if (equalityOperatorMethod != null)
            return BuildEqualityOperatorComparer(type, equalityOperatorMethod);

        // if Equals is overriden - return it's call
        var equalsMethod = ResolveEqualsMethod(type);
        if (equalsMethod != null)
            return BuildEqualsComparer(type, equalsMethod);

        // generic collections
        if (type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
            return BuildGenericEnumerableComparer(type, mapper);

        // non-generic collections
        if (type.GetInterfaces().Any(x => x == typeof(IEnumerable)))
            return BuildNonGenericEnumerableComparer(type, mapper);

        return BuildPropertyFieldComparer(type, mapper);
    }

    /// <summary>
    /// Adds reference equality checks to the comparer expression.
    /// </summary>
    /// <param name="a">The first parameter expression.</param>
    /// <param name="b">The second parameter expression.</param>
    /// <param name="returnTarget">The return target for early exit.</param>
    /// <returns>Expressions that perform reference equality checks.</returns>
    private static IEnumerable<Expression> AddReferenceEqualityChecks(
        ParameterExpression a,
        ParameterExpression b,
        LabelTarget returnTarget
    )
    {
        yield return Expression.IfThen(
            Expression.NotEqual(
                Expression.ReferenceEqual(a, Expression.Constant(null)),
                Expression.ReferenceEqual(b, Expression.Constant(null))
            ),
            Expression.Return(returnTarget, Expression.Constant(false))
        );

        yield return Expression.IfThen(
            Expression.ReferenceEqual(a, b),
            Expression.Return(returnTarget, Expression.Constant(true))
        );
    }
}
