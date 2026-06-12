using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Annium.Core.Mapper.Internal.Resolvers;

/// <summary>
/// Map resolver that creates mappings from dictionary sources to target types using property assignment
/// </summary>
internal class DictionaryAssignmentMapResolver : RepackerMapResolverBase, IMapResolver
{
    /// <summary>
    /// Initializes a new instance of the DictionaryAssignmentMapResolver class
    /// </summary>
    /// <param name="repacker">The expression repacker</param>
    public DictionaryAssignmentMapResolver(IRepacker repacker)
        : base(repacker) { }

    /// <summary>
    /// Determines whether this resolver can create a mapping between the specified source and target types
    /// </summary>
    /// <param name="src">The source type</param>
    /// <param name="tgt">The target type</param>
    /// <returns>True if the source is a string-object dictionary and target has a default constructor and is not enum, abstract, or interface, otherwise false</returns>
    public bool CanResolveMap(Type src, Type tgt) =>
        tgt.IsInstantiableTarget() && src.IsStringObjectDictionary() && tgt.GetConstructor(Type.EmptyTypes) is not null;

    /// <summary>
    /// Resolves and creates a mapping from dictionary source to target type using property assignment
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
            var tryGetValue = HelperExtensions.ResolveTryGetValue(src, tgt);
            var targets = tgt.GetWriteableProperties();

            // exclude configured/ignored members and explicit interface impls (shared with AssignmentMapResolver)
            targets = HelperExtensions.FilterAutoAssignTargets(cfg, targets);

            var body = new List<Expression>();
            HelperExtensions.AppendMemberMapAssignments(cfg, ctx, Repacker, source, instance, variables, body);

            // for each target property - resolve assignment expression
            body.AddRange(
                targets
                    .Select<PropertyInfo, Expression>(target =>
                    {
                        // resolve map for conversion and use it, if necessary
                        var map = ctx.ResolveMapping(typeof(object), target.PropertyType);

                        // otherwise - parameter must match respective source dictionary property
                        var item = HelperExtensions.BuildDictKeyAccess(source, tryGetValue, target.Name, variables);

                        return Expression.Assign(Expression.Property(instance, target), map(item));
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
