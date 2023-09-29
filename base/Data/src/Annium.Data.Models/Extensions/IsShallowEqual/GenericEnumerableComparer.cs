using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Annium.Core.Mapper;
using Annium.Reflection;

// ReSharper disable once CheckNamespace
namespace Annium.Data.Models.Extensions;

public static partial class IsShallowEqualExtensions
{
    private static LambdaExpression BuildGenericEnumerableComparer(Type type, IMapper mapper)
    {
        var elementType = type.GetTargetImplementation(typeof(IEnumerable<>))!.GenericTypeArguments[0];

        var a = Expression.Parameter(type);
        var b = Expression.Parameter(type);
        var parameters = new[] { a, b };

        var returnTarget = Expression.Label(typeof(bool));

        var vars = new List<ParameterExpression>();
        var expressions = new List<Expression>();

        if (type.IsClass)
            expressions.AddRange(AddReferenceEqualityChecks(a, b, returnTarget));

        var enumerableType = typeof(IEnumerable<>).MakeGenericType(elementType);
        var enumeratorType = typeof(IEnumerator<>).MakeGenericType(elementType);

        var getEnumerator = enumerableType.GetMethod(nameof(IEnumerable<object>.GetEnumerator))!;
        var moveNext = typeof(IEnumerator).GetMethod(nameof(IEnumerator.MoveNext))!;
        var current = enumeratorType.GetProperty(nameof(IEnumerator<object>.Current))!;

        // resolve enumerators
        var enumeratorAVar = Expression.Variable(enumeratorType);
        var enumeratorBVar = Expression.Variable(enumeratorType);
        vars.Add(enumeratorAVar);
        vars.Add(enumeratorBVar);
        expressions.Add(Expression.Assign(enumeratorAVar, Expression.Call(a, getEnumerator)));
        expressions.Add(Expression.Assign(enumeratorBVar, Expression.Call(b, getEnumerator)));

        var comparerVar = Expression.Variable(typeof(Func<,,>).MakeGenericType(elementType, elementType, typeof(bool)));
        vars.Add(comparerVar);
        expressions.Add(Expression.Assign(comparerVar, ResolveComparer(elementType, mapper)));

        var breakLabel = Expression.Label(typeof(void));
        expressions.Add(Expression.Loop(
            Expression.Block(
                // no next element in a - break
                Expression.IfThen(
                    Expression.Not(Expression.Call(enumeratorAVar, moveNext)),
                    Expression.Break(breakLabel)
                ),
                // no next element in b - different count, return false
                Expression.IfThen(
                    Expression.Not(Expression.Call(enumeratorBVar, moveNext)),
                    Expression.Return(returnTarget, Expression.Constant(false))
                ),
                Expression.IfThen(
                    Expression.Not(Expression.Invoke(
                        comparerVar,
                        Expression.Property(enumeratorAVar, current),
                        Expression.Property(enumeratorBVar, current)
                    )),
                    Expression.Return(returnTarget, Expression.Constant(false))
                )
            ),
            breakLabel
        ));

        // return true, if no next element in b (means same count)
        expressions.Add(Expression.Label(returnTarget, Expression.Not(Expression.Call(enumeratorBVar, moveNext))));

        return Expression.Lambda(Expression.Block(vars, expressions), parameters);
    }
}