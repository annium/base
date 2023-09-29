using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Annium.Core.Runtime.Types;

namespace Annium.Core.Mapper.Internal.Resolvers;

internal class ResolutionMapResolver : IMapResolver
{
    private readonly ITypeManager _typeManager;

    public ResolutionMapResolver(ITypeManager typeManager)
    {
        _typeManager = typeManager;
    }

    public bool CanResolveMap(Type src, Type tgt)
    {
        return tgt.IsAbstract || tgt.IsInterface;
    }

    public Mapping ResolveMap(Type src, Type tgt, IMapConfiguration cfg, IMapResolverContext ctx) => source =>
    {
        var vars = new List<ParameterExpression>();
        var expressions = new List<Expression>();

        var returnTarget = Expression.Label(tgt);

        // if source is default - return default target
        expressions.Add(Expression.IfThen(
            Expression.Equal(source, Expression.Default(src)),
            Expression.Return(returnTarget, Expression.Default(tgt))
        ));

        // add type resolution
        var typeVar = Expression.Variable(typeof(Type));
        vars.Add(typeVar);

        var resolve = typeof(ITypeManager)
            .GetMethod(nameof(ITypeManager.Resolve), new[] { typeof(object), typeof(Type) })!;

        expressions.Add(Expression.Assign(
            typeVar,
            Expression.Call(
                Expression.Constant(_typeManager),
                resolve,
                source, Expression.Constant(tgt)
            )
        ));

        // if type resolution failed - throw
        expressions.Add(Expression.IfThen(
            Expression.Equal(typeVar, Expression.Constant(null)),
            Expression.Throw(Expression.New(
                typeof(InvalidOperationException).GetConstructor(new[] { typeof(string) })!,
                Expression.Constant($"Can't resolve '{tgt}' implementation by signature of '{src}'")
            ))
        ));

        // get map
        var mapVar = Expression.Variable(typeof(Delegate));
        vars.Add(mapVar);

        var getMap = typeof(IMapResolverContext).GetMethod(nameof(IMapResolverContext.GetMap))!;
        var getTypeEx = Expression.Call(source, typeof(object).GetMethod(nameof(GetType))!);
        expressions.Add(Expression.Assign(mapVar, Expression.Call(Expression.Constant(ctx), getMap, getTypeEx, typeVar)));

        // invoke map and return result
        var invokeMap = typeof(Delegate).GetMethod(nameof(Delegate.DynamicInvoke))!;
        expressions.Add(Expression.Label(
            returnTarget,
            Expression.Convert(
                Expression.Call(mapVar, invokeMap, Expression.NewArrayInit(typeof(object), source)),
                tgt
            )
        ));

        return Expression.Block(vars, expressions);
    };
}