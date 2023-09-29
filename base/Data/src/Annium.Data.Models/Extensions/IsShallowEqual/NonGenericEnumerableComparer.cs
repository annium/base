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
    private static LambdaExpression BuildNonGenericEnumerableComparer(Type type, IMapper mapper)
    {
        var a = Expression.Parameter(type);
        var b = Expression.Parameter(type);
        var m = Expression.Constant(mapper);
        var parameters = new[] { a, b };

        var returnTarget = Expression.Label(typeof(bool));

        var vars = new List<ParameterExpression>();
        var expressions = new List<Expression>();

        if (type.IsClass)
            expressions.AddRange(AddReferenceEqualityChecks(a, b, returnTarget));

        var enumerableType = typeof(IEnumerable);
        var enumeratorType = typeof(IEnumerator);

        var getEnumerator = enumerableType.GetMethod(nameof(IEnumerable.GetEnumerator))!;
        var moveNext = enumeratorType.GetMethod(nameof(IEnumerator.MoveNext))!;
        var current = enumeratorType.GetProperty(nameof(IEnumerator.Current))!;

        // resolve enumerators
        var enumeratorAVar = Expression.Variable(enumeratorType);
        var enumeratorBVar = Expression.Variable(enumeratorType);
        vars.Add(enumeratorAVar);
        vars.Add(enumeratorBVar);
        expressions.Add(Expression.Assign(enumeratorAVar, Expression.Call(a, getEnumerator)));
        expressions.Add(Expression.Assign(enumeratorBVar, Expression.Call(b, getEnumerator)));

        var comparerMethod = typeof(IsShallowEqualExtensions).GetMethods()
            .Single(x => x.GetParameters().Length == 3)
            .MakeGenericMethod(typeof(object), typeof(object));

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
                    Expression.Not(Expression.Call(
                        null,
                        comparerMethod,
                        Expression.Property(enumeratorAVar, current),
                        Expression.Property(enumeratorBVar, current),
                        m
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