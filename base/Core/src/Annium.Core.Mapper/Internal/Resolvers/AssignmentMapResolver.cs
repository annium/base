using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Annium.Core.Mapper.Internal.Resolvers;

/// <summary>
/// Map resolver that creates mappings using property assignment for types with default constructors
/// </summary>
internal class AssignmentMapResolver : IMapResolver
{
    /// <summary>
    /// The expression repacker for repackaging expressions
    /// </summary>
    private readonly IRepacker _repacker;

    /// <summary>
    /// Initializes a new instance of the AssignmentMapResolver class
    /// </summary>
    /// <param name="repacker">The expression repacker</param>
    public AssignmentMapResolver(IRepacker repacker)
    {
        _repacker = repacker;
    }

    /// <summary>
    /// Determines whether this resolver can create a mapping between the specified source and target types
    /// </summary>
    /// <param name="src">The source type</param>
    /// <param name="tgt">The target type</param>
    /// <returns>True if the target type has a default constructor, otherwise false</returns>
    public bool CanResolveMap(Type src, Type tgt) => tgt.GetConstructor(Type.EmptyTypes) is not null;

    /// <summary>
    /// Resolves and creates a mapping between the specified source and target types using property assignment
    /// </summary>
    /// <param name="src">The source type</param>
    /// <param name="tgt">The target type</param>
    /// <param name="cfg">The mapping configuration</param>
    /// <param name="ctx">The resolver context</param>
    /// <returns>The resolved mapping</returns>
    public Mapping ResolveMap(Type src, Type tgt, IMapConfiguration cfg, IMapResolverContext ctx) =>
        source =>
        {
            // defined instance and create initial assignment expression
            var variables = new List<ParameterExpression>();
            var instance = Expression.Variable(tgt);
            variables.Add(instance);
            var constructor = tgt.GetDefaultConstructor();
            var init = Expression.Assign(instance, Expression.New(constructor));

            // get source and target type properties
            var sources = src.GetReadableProperties();
            var targets = tgt.GetWriteableProperties();

            // exclude target properties, that are configured to be ignored or have configured mapping, from basic assignment mapping
            var excludedMembers = cfg.MemberMaps.Keys.Concat(cfg.IgnoredMembers).ToArray();
            targets = targets
                .Where(target =>
                    !excludedMembers.Any(x =>
                        x.DeclaringType == target.DeclaringType
                        && x.PropertyType == target.PropertyType
                        && x.Name == target.Name
                    )
                )
                // ignore interface implementations
                .Where(x => !x.Name.Contains('.'))
                .ToArray();

            var body = new List<Expression>();
            foreach (var group in cfg.MemberMaps.GroupBy(x => x.Value))
            {
                var map = group.Key(ctx.MapContext.Value);
                var members = group.Select(x => x.Key).ToArray();

                if (members.Length == 1)
                    body.Add(
                        Expression.Assign(
                            Expression.Property(instance, members.Single()),
                            _repacker.Repack(map.Body)(source)
                        )
                    );
                else
                {
                    var variable = Expression.Variable(map.Body.Type);
                    variables.Add(variable);
                    body.Add(Expression.Assign(variable, _repacker.Repack(map.Body)(source)));

                    foreach (var member in members)
                        body.Add(
                            Expression.Assign(
                                Expression.Property(instance, member),
                                Expression.Property(variable, map.Body.Type, member.Name)
                            )
                        );
                }
            }

            // for each target property - resolve assignment expression
            body.AddRange(
                targets
                    .Select<PropertyInfo, Expression>(target =>
                    {
                        // otherwise - target field must match respective source field
                        var prop =
                            sources.FirstOrDefault(p =>
                                string.Equals(p.Name, target.Name, StringComparison.InvariantCultureIgnoreCase)
                            )
                            ?? throw new MappingException(src, tgt, $"No property found for target property {target}");

                        // resolve map for conversion and use it, if necessary
                        var map = ctx.ResolveMapping(prop.PropertyType, target.PropertyType);

                        return Expression.Assign(
                            Expression.Property(instance, target),
                            map(Expression.Property(source, prop))
                        );
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

            var nullCheck = Expression.IfThen(Expression.Equal(source, Expression.Default(src)), returnExpression);

            var result = Expression.Return(returnTarget, instance, tgt);

            return Expression.Block(
                variables,
                new Expression[] { nullCheck, init }
                    .Concat(body)
                    .Concat(new Expression[] { result, returnLabel })
            );
        };
}
