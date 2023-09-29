using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Annium.Data.Models.Extensions;

public static partial class IsShallowEqualExtensions
{
    private static MethodInfo? ResolveEqualityOperatorMethod(Type type) => type.GetMethods()
        .SingleOrDefault(x =>
            x is { IsSpecialName: true, Name: "op_Equality" } &&
            x.GetParameters().All(y => y.ParameterType == type)
        );

    private static LambdaExpression BuildEqualityOperatorComparer(Type type, MethodInfo equalityOperatorMethod)
    {
        var a = Expression.Parameter(type);
        var b = Expression.Parameter(type);

        return Expression.Lambda(Expression.Call(null, equalityOperatorMethod, a, b), a, b);
    }
}