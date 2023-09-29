using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Annium.Core.Mapper.Internal.Resolvers;

internal class EnumerableMapResolver : IMapResolver
{
    public bool CanResolveMap(Type src, Type tgt)
    {
        return src.GetEnumerableElementType() != null && tgt.GetEnumerableElementType() != null;
    }

    public Mapping ResolveMap(Type src, Type tgt, IMapConfiguration cfg, IMapResolverContext ctx) => source =>
    {
        var srcEl = src.GetEnumerableElementType()!;
        var tgtEl = tgt.GetEnumerableElementType()!;

        // if tgt is interface - resolve container type
        if (tgt.IsInterface)
        {
            var def = tgt.GetGenericTypeDefinition();
            if (def == typeof(ICollection<>) || def == typeof(IReadOnlyCollection<>) || def == typeof(IEnumerable<>))
                tgt = tgtEl.MakeArrayType();
            if (def == typeof(IList<>) || def == typeof(IReadOnlyList<>))
                tgt = typeof(List<>).MakeGenericType(tgt.GenericTypeArguments);
            if (def == typeof(IDictionary<,>) || def == typeof(IReadOnlyDictionary<,>))
                tgt = typeof(Dictionary<,>).MakeGenericType(tgt.GenericTypeArguments);
        }

        var select = typeof(Enumerable).GetMethods()
            .First(m => m.Name == nameof(Enumerable.Select))
            .MakeGenericMethod(srcEl, tgtEl);
        var selectLambda = BuildSelectLambda(srcEl, tgtEl, ctx);
        var selection = Expression.Call(select, source, selectLambda);
        var toArray = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray))!.MakeGenericMethod(tgtEl);
        var result = Expression.Condition(
            Expression.Equal(source, Expression.Default(src)),
            Expression.NewArrayInit(tgtEl),
            Expression.Call(toArray, selection)
        );

        if (tgt.IsArray)
            return result;

        var parameter = typeof(IEnumerable<>).MakeGenericType(tgtEl);
        var constructor = tgt.GetConstructor(new[] { parameter });
        if (constructor is null)
            throw new MappingException(src, tgt, $"No constructor with single {parameter} parameter found.");

        return Expression.New(constructor, selection);
    };

    private LambdaExpression BuildSelectLambda(Type srcEl, Type tgtEl, IMapResolverContext ctx)
    {
        var param = Expression.Parameter(srcEl);
        var vars = new List<ParameterExpression>();
        var body = new List<Expression>();
        var returnTarget = Expression.Label(tgtEl);

        // if param is default - return default target element
        if (!srcEl.IsValueType)
            body.Add(Expression.IfThen(
                Expression.Equal(param, Expression.Default(srcEl)),
                Expression.Return(returnTarget, Expression.Default(tgtEl))
            ));

        // get map for element type
        var mapVar = Expression.Variable(typeof(Delegate));
        vars.Add(mapVar);
        var getMap = typeof(IMapResolverContext).GetMethod(nameof(IMapResolverContext.GetMap))!;
        var getTypeEx = Expression.Call(param, typeof(object).GetMethod(nameof(GetType))!);
        body.Add(Expression.Assign(mapVar, Expression.Call(Expression.Constant(ctx), getMap, getTypeEx, Expression.Constant(tgtEl))));

        // invoke map and return result
        var invokeMap = typeof(Delegate).GetMethod(nameof(Delegate.DynamicInvoke))!;
        body.Add(Expression.Label(
            returnTarget,
            Expression.Convert(
                Expression.Call(mapVar, invokeMap, Expression.NewArrayInit(typeof(object), Expression.Convert(param, typeof(object)))),
                tgtEl
            )
        ));

        return Expression.Lambda(Expression.Block(vars, body), param);
    }
}