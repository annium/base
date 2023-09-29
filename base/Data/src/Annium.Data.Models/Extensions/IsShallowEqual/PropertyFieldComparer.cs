using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Annium.Core.Mapper;
using Annium.Reflection;

// ReSharper disable once CheckNamespace
namespace Annium.Data.Models.Extensions;

public static partial class IsShallowEqualExtensions
{
    private static LambdaExpression BuildPropertyFieldComparer(Type type, IMapper mapper)
    {
        var a = Expression.Parameter(type);
        var b = Expression.Parameter(type);
        var parameters = new List<ParameterExpression> { a, b };

        var returnTarget = Expression.Label(typeof(bool));

        var vars = new List<ParameterExpression>();
        var expressions = new List<Expression>();

        if (type.IsClass)
            expressions.AddRange(AddReferenceEqualityChecks(a, b, returnTarget));

        var propertyExpressions = type
            .GetAllProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(x => x.CanRead && x.GetIndexParameters().Length == 0)
            .ToArray();
        foreach (var property in propertyExpressions)
            RegisterMember(property.PropertyType, Expression.Property(a, property), Expression.Property(b, property));

        var fieldExpressions = type
            .GetAllFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var field in fieldExpressions)
            RegisterMember(field.FieldType, Expression.Field(a, field), Expression.Field(b, field));

        expressions.Add(Expression.Label(returnTarget, Expression.Constant(true)));

        return Expression.Lambda(Expression.Block(vars, expressions), parameters);

        void RegisterMember(Type memberType, MemberExpression ax, MemberExpression bx)
        {
            var comparer = ResolveComparer(memberType, mapper);
            var comparerVar = Expression.Variable(comparer.Type);

            vars.Add(comparerVar);
            expressions.Add(Expression.Assign(comparerVar, comparer));

            expressions.Add(Expression.IfThen(
                Expression.Not(Expression.Invoke(comparerVar, ax, bx)),
                Expression.Return(returnTarget, Expression.Constant(false))
            ));
        }
    }
}