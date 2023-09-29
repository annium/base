using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Annium.Reflection;

public static class TypeHelper
{
    public static LambdaExpression[] GetAccessExpressions<T>(Expression<Func<T, object>> map)
    {
        if (map.Body is NewExpression create)
            return create.Arguments
                .Select(x => Expression.Lambda(x, map.Parameters))
                .ToArray();

        return new LambdaExpression[] { map };
    }

    public static PropertyInfo[] ResolveProperties<T>(Expression<Func<T, object>> map)
    {
        if (map.Body is NewExpression create)
            return create.Arguments.Select(ResolveProperty).ToArray();

        return new[] { ResolveProperty(map.Body) };
    }

    public static PropertyInfo ResolveProperty<T, TV>(Expression<Func<T, TV>> map)
        => ResolveProperty(map.Body);

    public static PropertyInfo ResolveProperty<T>(Expression<Func<T, object>> map)
        => ResolveProperty(map.Body);

    private static PropertyInfo ResolveProperty(Expression ex)
    {
        if (ex is MemberExpression member)
            return ResolveProperty(member);

        if (ex is UnaryExpression unary)
            return ResolveProperty(unary);

        throw new ArgumentException($"Can't resolve property from {ex}");
    }

    private static PropertyInfo ResolveProperty(MemberExpression ex)
    {
        if (ex.Member is PropertyInfo property)
            return property;

        throw new ArgumentException($"{ex} is not a property access expression");
    }

    private static PropertyInfo ResolveProperty(UnaryExpression ex)
    {
        if (ex.Operand is MemberExpression member)
            return ResolveProperty(member);

        throw new ArgumentException($"{ex} is not a property access expression");
    }
}