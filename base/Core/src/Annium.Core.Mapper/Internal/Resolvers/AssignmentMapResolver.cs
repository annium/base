using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Annium.Core.Mapper.Internal.Resolvers;

/// <summary>
/// Map resolver that creates mappings using property assignment for types with default constructors
/// </summary>
internal class AssignmentMapResolver : RepackerMapResolverBase, IMapResolver
{
    /// <summary>
    /// Initializes a new instance of the AssignmentMapResolver class
    /// </summary>
    /// <param name="repacker">The expression repacker</param>
    public AssignmentMapResolver(IRepacker repacker)
        : base(repacker) { }

    /// <summary>
    /// Determines whether this resolver can create a mapping between the specified source and target types
    /// </summary>
    /// <param name="src">The source type</param>
    /// <param name="tgt">The target type</param>
    /// <returns>True if the target type has a default constructor and is not enum, abstract, or interface, otherwise false</returns>
    public bool CanResolveMap(Type src, Type tgt) =>
        tgt.IsInstantiableTarget() && tgt.GetConstructor(Type.EmptyTypes) is not null;

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
            var (variables, instance, init) = HelperExtensions.BuildDefaultConstructorInit(tgt);

            // get source and target type properties
            var sources = src.GetReadableProperties();
            var targets = tgt.GetWriteableProperties();

            // exclude target properties configured to be ignored / explicitly mapped, and explicit interface
            // implementations, from basic assignment mapping (shared with DictionaryAssignmentMapResolver)
            targets = HelperExtensions.FilterAutoAssignTargets(cfg, targets);

            var body = new List<Expression>();
            HelperExtensions.AppendMemberMapAssignments(cfg, ctx, Repacker, source, instance, variables, body);

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

            return HelperExtensions.BuildResolvedBlock(
                src,
                tgt,
                source,
                variables,
                new Expression[] { init },
                body,
                instance
            );
        };
}
