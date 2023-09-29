using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Annium.Reflection;

namespace Annium.Data.Tables.Internal;

internal static class TableHelper
{
    public static Func<T, int> BuildGetKey<T>(Expression<Func<T, object>> getKeyExpression)
    {
        var expressions = TypeHelper.GetAccessExpressions(getKeyExpression);
        if (expressions.Length == 0)
            throw new ArgumentException($"Invalid key expression: {getKeyExpression}");

        var method = typeof(HashCode).GetMethods()
            .Single(x => x.Name == nameof(HashCode.Combine) && x.GetParameters().Length == expressions.Length)
            .MakeGenericMethod(expressions.Select(x => x.ReturnType).ToArray());
        var body = Expression.Call(null!, method, expressions.Select(x => x.Body).ToArray());

        var lambda = Expression.Lambda(body, getKeyExpression.Parameters);

        return (Func<T, int>)lambda.Compile();
    }

    public static Action<T, T> BuildUpdate<T>(TablePermission permissions)
    {
        if (!permissions.HasFlag(TablePermission.Update))
            return (_, _) => { };

        var row = Expression.Parameter(typeof(T), "row");
        var upd = Expression.Parameter(typeof(T), "upd");
        var properties = typeof(T).GetProperties().Where(x => x.CanWrite).ToArray();
        if (properties.Length == 0)
            throw new InvalidOperationException($"Table write type {typeof(T).Name} has no writable properties.");

        var expressions = properties.Select<PropertyInfo, Expression>(prop =>
        {
            var rowValue = Expression.Property(row, prop);
            var updValue = Expression.Property(upd, prop);

            // if not nullable value type - assign always
            if (prop.PropertyType.IsNotNullableValueType())
                return Expression.Assign(rowValue, updValue);

            // if nullable value type or reference type - assign if not null
            return Expression.IfThen(
                Expression.NotEqual(updValue, Expression.Constant(null)),
                Expression.Assign(rowValue, updValue)
            );
        }).ToArray();

        var lambda = Expression.Lambda<Action<T, T>>(
            Expression.Block(expressions),
            row, upd
        );

        return lambda.Compile();
    }
}