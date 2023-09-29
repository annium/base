using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Annium.Core.Mapper;

// ReSharper disable once CheckNamespace
namespace Annium.Data.Models.Extensions;

public static partial class IsShallowEqualExtensions
{
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