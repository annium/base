using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Annium.Core.Mapper.Internal.Resolvers;

internal class ConstructorMapResolver : IMapResolver
{
    private readonly IRepacker _repacker;

    public ConstructorMapResolver(IRepacker repacker)
    {
        _repacker = repacker;
    }

    public bool CanResolveMap(Type src, Type tgt)
    {
        if (tgt.IsEnum || tgt.IsAbstract || tgt.IsInterface)
            return false;

        return tgt.GetConstructor(Type.EmptyTypes) is null;
    }

    public Mapping ResolveMap(Type src, Type tgt, IMapConfiguration cfg, IMapResolverContext ctx) =>
        source =>
        {
            // find constructor with biggest number of parameters (pretty simple logic for now)
            var constructor = tgt.GetParametrizedConstructor();

            // get source properties and constructor parameters
            var sources = src.GetReadableProperties();
            var parameters = constructor.GetParameters();

            var body = new List<Expression>();

            var variables = new List<ParameterExpression>();
            var mappedMemberVars = new Dictionary<string, ParameterExpression>();
            foreach (var group in cfg.MemberMaps.GroupBy(x => x.Value))
            {
                var map = group.Key(ctx.MapContext.Value);
                var members = group.Select(x => x.Key).ToArray();

                if (members.Length == 1)
                {
                    var member = members.Single();
                    var memberVar = Expression.Variable(member.PropertyType);
                    variables.Add(memberVar);
                    body.Add(Expression.Assign(memberVar, _repacker.Repack(map.Body)(source)));
                    mappedMemberVars[member.Name.ToLowerInvariant()] = memberVar;
                }
                else
                {
                    var resultVar = Expression.Variable(map.Body.Type);
                    variables.Add(resultVar);
                    body.Add(Expression.Assign(resultVar, _repacker.Repack(map.Body)(source)));

                    foreach (var member in members)
                    {
                        var memberVar = Expression.Variable(member.PropertyType);
                        variables.Add(memberVar);
                        body.Add(
                            Expression.Assign(memberVar, Expression.Property(resultVar, map.Body.Type, member.Name))
                        );
                        mappedMemberVars[member.Name.ToLowerInvariant()] = memberVar;
                    }
                }
            }

            // map parameters to their value evaluation expressions
            var ignoredMembers = cfg.IgnoredMembers.Select(x => x.Name.ToLowerInvariant()).ToArray();
            var mappedMembers = cfg.MemberMaps.Keys.Select(x => x.Name.ToLowerInvariant()).ToArray();
            var values = parameters
                .Select(param =>
                {
                    var paramName = param.Name!.ToLowerInvariant();

                    // if respective property is ignored - use default value for parameter
                    if (ignoredMembers.Contains(paramName))
                        return Expression.Default(param.ParameterType);

                    // if respective property is mapped - use variable, containing it's value
                    if (mappedMembers.Contains(paramName))
                        return mappedMemberVars[paramName];

                    // otherwise - parameter must match respective source field
                    var prop =
                        sources.FirstOrDefault(p => p.Name.ToLowerInvariant() == paramName)
                        ?? throw new MappingException(src, tgt, $"No property found for constructor parameter {param}");

                    // resolve map for conversion and use it, if necessary
                    var map = ctx.ResolveMapping(prop.PropertyType, param.ParameterType);

                    return map(Expression.Property(source, prop));
                })
                .ToArray();

            var instance = Expression.New(constructor, values);

            // if src is struct - things are simpler, no null-checking
            if (src.IsValueType)
                return Expression.Block(variables, body.Concat(new[] { instance }));

            // define labeled return expression, that will express early return null-checking statement
            var returnTarget = Expression.Label(tgt);
            var defaultValue = Expression.Default(tgt);
            var returnExpression = Expression.Return(returnTarget, defaultValue, tgt);
            var returnLabel = Expression.Label(returnTarget, defaultValue);

            var nullCheck = Expression.IfThen(Expression.Equal(source, Expression.Default(src)), returnExpression);

            var result = Expression.Return(returnTarget, instance, tgt);

            return Expression.Block(
                variables,
                new Expression[] { nullCheck }
                    .Concat(body)
                    .Concat(new Expression[] { result, returnLabel })
            );
        };
}
