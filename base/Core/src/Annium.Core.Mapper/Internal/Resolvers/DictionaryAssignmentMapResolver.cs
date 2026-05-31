using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Annium.Core.Mapper.Internal.Resolvers;

/// <summary>
/// Map resolver that creates mappings from dictionary sources to target types using property assignment
/// </summary>
internal class DictionaryAssignmentMapResolver : IMapResolver
{
    /// <summary>
    /// The expression repacker for repackaging expressions
    /// </summary>
    private readonly IRepacker _repacker;

    /// <summary>
    /// Initializes a new instance of the DictionaryAssignmentMapResolver class
    /// </summary>
    /// <param name="repacker">The expression repacker</param>
    public DictionaryAssignmentMapResolver(IRepacker repacker)
    {
        _repacker = repacker;
    }

    /// <summary>
    /// Determines whether this resolver can create a mapping between the specified source and target types
    /// </summary>
    /// <param name="src">The source type</param>
    /// <param name="tgt">The target type</param>
    /// <returns>True if the source is a string-object dictionary and target has a default constructor, otherwise false</returns>
    public bool CanResolveMap(Type src, Type tgt) =>
        src.IsStringObjectDictionary() && tgt.GetConstructor(Type.EmptyTypes) is not null;

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
            var variables = new List<ParameterExpression>();
            var instance = Expression.Variable(tgt);
            variables.Add(instance);
            var constructor = tgt.GetDefaultConstructor();
            var init = Expression.Assign(instance, Expression.New(constructor));

            // get source and target type properties
            var tryGetValue = HelperExtensions.ResolveTryGetValue(src, tgt);
            var targets = tgt.GetWriteableProperties();

            // exclude configured/ignored members and explicit interface impls (shared with AssignmentMapResolver)
            targets = HelperExtensions.FilterAutoAssignTargets(cfg, targets);

            var body = new List<Expression>();
            HelperExtensions.AppendMemberMapAssignments(cfg, ctx, _repacker, source, instance, variables, body);

            // for each target property - resolve assignment expression
            body.AddRange(
                targets
                    .Select<PropertyInfo, Expression>(target =>
                    {
                        // resolve map for conversion and use it, if necessary
                        var map = ctx.ResolveMapping(typeof(object), target.PropertyType);

                        // otherwise - parameter must match respective source dictionary property
                        var itemVar = Expression.Variable(typeof(object));
                        variables.Add(itemVar);
                        var item = Expression.Condition(
                            Expression.Call(source, tryGetValue, Expression.Constant(target.Name), itemVar),
                            itemVar,
                            Expression.Throw(
                                Expression.New(
                                    typeof(KeyNotFoundException).GetConstructor(new[] { typeof(string) }).NotNull(),
                                    Expression.Constant($"Missing value for property '{target.Name}'")
                                ),
                                typeof(object)
                            )
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

            var (nullCheck, result, returnLabel) = HelperExtensions.BuildNullCheckedReturn(src, tgt, source, instance);
            return Expression.Block(
                variables,
                new Expression[] { nullCheck, init }
                    .Concat(body)
                    .Concat(new Expression[] { result, returnLabel })
            );
        };
}
