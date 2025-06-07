using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Annium.Reflection;

/// <summary>
/// Provides helper methods for working with types and expressions.
/// </summary>
public static class TypeHelper
{
    /// <summary>
    /// Gets an array of lambda expressions from a mapping expression.
    /// </summary>
    /// <typeparam name="T">The type of the input parameter.</typeparam>
    /// <param name="map">The mapping expression.</param>
    /// <returns>An array of lambda expressions.</returns>
    public static LambdaExpression[] GetAccessExpressions<T>(Expression<Func<T, object>> map)
    {
        if (map.Body is NewExpression create)
            return create.Arguments.Select(x => Expression.Lambda(x, map.Parameters)).ToArray();

        return new LambdaExpression[] { map };
    }

    /// <summary>
    /// Resolves property information from a mapping expression.
    /// </summary>
    /// <typeparam name="T">The type of the input parameter.</typeparam>
    /// <param name="map">The mapping expression.</param>
    /// <returns>An array of property information.</returns>
    public static PropertyInfo[] ResolveProperties<T>(Expression<Func<T, object>> map)
    {
        if (map.Body is NewExpression create)
            return create.Arguments.Select(ResolveProperty).ToArray();

        return new[] { ResolveProperty(map.Body) };
    }

    /// <summary>
    /// Resolves property information from a mapping expression with a specific return type.
    /// </summary>
    /// <typeparam name="T">The type of the input parameter.</typeparam>
    /// <typeparam name="TV">The type of the return value.</typeparam>
    /// <param name="map">The mapping expression.</param>
    /// <returns>The property information.</returns>
    public static PropertyInfo ResolveProperty<T, TV>(Expression<Func<T, TV>> map) => ResolveProperty(map.Body);

    /// <summary>
    /// Resolves property information from a mapping expression.
    /// </summary>
    /// <typeparam name="T">The type of the input parameter.</typeparam>
    /// <param name="map">The mapping expression.</param>
    /// <returns>The property information.</returns>
    public static PropertyInfo ResolveProperty<T>(Expression<Func<T, object>> map) => ResolveProperty(map.Body);

    /// <summary>
    /// Resolves property information from an expression.
    /// </summary>
    /// <param name="ex">The expression to resolve.</param>
    /// <returns>The property information.</returns>
    /// <exception cref="ArgumentException">Thrown when the expression cannot be resolved to a property.</exception>
    private static PropertyInfo ResolveProperty(Expression ex)
    {
        if (ex is MemberExpression member)
            return ResolveProperty(member);

        if (ex is UnaryExpression unary)
            return ResolveProperty(unary);

        throw new ArgumentException($"Can't resolve property from {ex}");
    }

    /// <summary>
    /// Resolves property information from a member expression.
    /// </summary>
    /// <param name="ex">The member expression to resolve.</param>
    /// <returns>The property information.</returns>
    /// <exception cref="ArgumentException">Thrown when the expression is not a property access expression.</exception>
    private static PropertyInfo ResolveProperty(MemberExpression ex)
    {
        if (ex.Member is PropertyInfo property)
            return property;

        throw new ArgumentException($"{ex} is not a property access expression");
    }

    /// <summary>
    /// Resolves property information from a unary expression.
    /// </summary>
    /// <param name="ex">The unary expression to resolve.</param>
    /// <returns>The property information.</returns>
    /// <exception cref="ArgumentException">Thrown when the expression is not a property access expression.</exception>
    private static PropertyInfo ResolveProperty(UnaryExpression ex)
    {
        if (ex.Operand is MemberExpression member)
            return ResolveProperty(member);

        throw new ArgumentException($"{ex} is not a property access expression");
    }
}
