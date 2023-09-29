using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Annium.Core.Mapper.Internal.Resolvers;

internal class DictionaryAssignmentMapResolver : IMapResolver
{
    private readonly IRepacker _repacker;

    public DictionaryAssignmentMapResolver(IRepacker repacker)
    {
        _repacker = repacker;
    }

    public bool CanResolveMap(Type src, Type tgt)
    {
        return (
                src == typeof(Dictionary<string, object>) ||
                src == typeof(IDictionary<string, object>) ||
                src == typeof(IReadOnlyDictionary<string, object>)
            ) &&
            tgt.GetConstructor(Type.EmptyTypes) is not null;
    }

    public Mapping ResolveMap(Type src, Type tgt, IMapConfiguration cfg, IMapResolverContext ctx) => source =>
    {
        // defined instance and create initial assignment expression
        var variables = new List<ParameterExpression>();
        var instance = Expression.Variable(tgt);
        variables.Add(instance);
        var constructor = tgt.GetDefaultConstructor();
        var init = Expression.Assign(instance, Expression.New(constructor));

        // get source and target type properties
        var tryGetValue = src.GetMethod(nameof(Dictionary<object, object>.TryGetValue))
            ?? throw new MappingException(src, tgt, $"Failed to resolve method {src.FriendlyName()}.{nameof(Dictionary<object, object>.TryGetValue)}");
        var targets = tgt.GetWriteableProperties();

        // exclude target properties, that are configured to be ignored or have configured mapping, from basic assignment mapping
        var excludedMembers = cfg.MemberMaps.Keys.Concat(cfg.IgnoredMembers).ToArray();
        targets = targets
            .Where(target => !excludedMembers.Any(x =>
                x.DeclaringType == target.DeclaringType &&
                x.PropertyType == target.PropertyType &&
                x.Name == target.Name
            ))
            // ignore interface implementations
            .Where(x => !x.Name.Contains('.'))
            .ToArray();

        var body = new List<Expression>();
        foreach (var group in cfg.MemberMaps.GroupBy(x => x.Value))
        {
            var map = group.Key(ctx.MapContext.Value);
            var members = group.Select(x => x.Key).ToArray();

            if (members.Length == 1)
                body.Add(Expression.Assign(Expression.Property(instance, members.Single()), _repacker.Repack(map.Body)(source)));
            else
            {
                var variable = Expression.Variable(map.Body.Type);
                variables.Add(variable);
                body.Add(Expression.Assign(variable, _repacker.Repack(map.Body)(source)));

                foreach (var member in members)
                    body.Add(Expression.Assign(
                        Expression.Property(instance, member),
                        Expression.Property(variable, map.Body.Type, member.Name)
                    ));
            }
        }

        // for each target property - resolve assignment expression
        body.AddRange(targets
            .Select<PropertyInfo, Expression>(target =>
            {
                // resolve map for conversion and use it, if necessary
                var map = ctx.ResolveMapping(typeof(object), target.PropertyType);

                // otherwise - parameter must match respective source dictionary property
                var itemVar = Expression.Variable(typeof(object));
                var item = Expression.Condition(
                    Expression.Call(source, tryGetValue, Expression.Constant(target.Name), itemVar),
                    itemVar,
                    Expression.Throw(Expression.New(
                        typeof(KeyNotFoundException).GetConstructor(new[] { typeof(string) })!,
                        Expression.Constant($"Missing value for property '{target.Name}'")
                    ))
                );

                return Expression.Assign(Expression.Property(instance, target), map(item));
            })
            .ToArray()
        );

        // if src is struct - things are simpler, no null-checking
        if (src.IsValueType)
            return Expression.Block(
                variables,
                new Expression[] { init }
                    .Concat(body)
                    .Concat(new Expression[] { instance })
            );

        // define labeled return expression, that will express early return null-checking statement
        var returnTarget = Expression.Label(tgt);
        var defaultValue = Expression.Default(tgt);
        var returnExpression = Expression.Return(returnTarget, defaultValue, tgt);
        var returnLabel = Expression.Label(returnTarget, defaultValue);

        var nullCheck = Expression.IfThen(
            Expression.Equal(source, Expression.Default(src)),
            returnExpression
        );

        var result = Expression.Return(returnTarget, instance, tgt);

        return Expression.Block(
            variables,
            new Expression[] { nullCheck, init }
                .Concat(body)
                .Concat(new Expression[] { result, returnLabel })
        );
    };
}