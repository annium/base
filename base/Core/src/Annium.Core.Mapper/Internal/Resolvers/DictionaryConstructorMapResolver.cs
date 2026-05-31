using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Annium.Core.Mapper.Internal.Resolvers;

/// <summary>
/// Map resolver that creates mappings from dictionary sources to target types using constructor parameters
/// </summary>
internal class DictionaryConstructorMapResolver : IMapResolver
{
    /// <summary>
    /// The expression repacker for repackaging expressions
    /// </summary>
    private readonly IRepacker _repacker;

    /// <summary>
    /// Initializes a new instance of the DictionaryConstructorMapResolver class
    /// </summary>
    /// <param name="repacker">The expression repacker</param>
    public DictionaryConstructorMapResolver(IRepacker repacker)
    {
        _repacker = repacker;
    }

    /// <summary>
    /// Determines whether this resolver can create a mapping between the specified source and target types
    /// </summary>
    /// <param name="src">The source type</param>
    /// <param name="tgt">The target type</param>
    /// <returns>True if the source is a string-object dictionary and target has no default constructor, otherwise false</returns>
    public bool CanResolveMap(Type src, Type tgt) =>
        // mirror ConstructorMapResolver's target guards: enum / abstract / interface targets have no
        // usable parameterized constructor, so reject them here rather than throwing from GetParametrizedConstructor
        !tgt.IsAbstract
        && !tgt.IsInterface
        && !tgt.IsEnum
        && src.IsStringObjectDictionary()
        && tgt.GetConstructor(Type.EmptyTypes) is null;

    /// <summary>
    /// Resolves and creates a mapping from dictionary source to target type using constructor parameters
    /// </summary>
    /// <param name="src">The source type</param>
    /// <param name="tgt">The target type</param>
    /// <param name="cfg">The mapping configuration</param>
    /// <param name="ctx">The resolver context</param>
    /// <returns>The resolved mapping</returns>
    public Mapping ResolveMap(Type src, Type tgt, IMapConfiguration cfg, IMapResolverContext ctx) =>
        source =>
        {
            // find constructor with biggest number of parameters (pretty simple logic for now)
            var constructor = tgt.GetParametrizedConstructor();

            // get source accessor and constructor parameters
            var tryGetValue = HelperExtensions.ResolveTryGetValue(src, tgt);
            var parameters = constructor.GetParameters();

            // resolve each constructor parameter to the matching target property's name (PascalCase),
            // so the dictionary key matches DictionaryAssignmentMapResolver's `target.Name` convention;
            // falls back to the parameter name when no matching property is found.
            var targetProperties = tgt.GetWriteableProperties();
            var paramKey = parameters.ToDictionary(
                p => p.Name.NotNull(),
                p =>
                    targetProperties
                        .FirstOrDefault(prop => string.Equals(prop.Name, p.Name, StringComparison.OrdinalIgnoreCase))
                        ?.Name
                    ?? p.Name.NotNull()
            );

            var body = new List<Expression>();

            var variables = new List<ParameterExpression>();
            var mappedMemberVars = new Dictionary<string, ParameterExpression>();
            HelperExtensions.AppendMemberMapVariables(cfg, ctx, _repacker, source, variables, body, mappedMemberVars);

            // map parameters to their value evaluation expressions
            var ignoredMembers = cfg.IgnoredMembers.Select(x => x.Name.ToLowerInvariant()).ToArray();
            var mappedMembers = cfg.MemberMaps.Keys.Select(x => x.Name.ToLowerInvariant()).ToArray();
            var values = parameters
                .Select(param =>
                {
                    // ParameterInfo.Name is null only for return-value parameters; constructor parameters always have names
                    var paramName = param.Name!;
                    var paramNameLow = paramName.ToLowerInvariant();

                    // if respective property is ignored - use default value for parameter
                    if (ignoredMembers.Contains(paramNameLow))
                        return Expression.Default(param.ParameterType);

                    // if respective property is mapped - use variable, containing it's value
                    if (mappedMembers.Contains(paramNameLow))
                        return mappedMemberVars[paramNameLow];

                    // resolve map for conversion and use it, if necessary
                    var map = ctx.ResolveMapping(typeof(object), param.ParameterType);

                    // otherwise - parameter must match respective source dictionary property
                    var itemVar = Expression.Variable(typeof(object));
                    variables.Add(itemVar);
                    var dictKey = paramKey[paramName];
                    var item = Expression.Condition(
                        Expression.Call(source, tryGetValue, Expression.Constant(dictKey), itemVar),
                        itemVar,
                        Expression.Throw(
                            Expression.New(
                                typeof(KeyNotFoundException).GetConstructor(new[] { typeof(string) }).NotNull(),
                                Expression.Constant($"Missing value for property '{dictKey}'")
                            ),
                            typeof(object)
                        )
                    );

                    return map(item);
                })
                .ToArray();

            var instance = Expression.New(constructor, values);

            // if src is struct - things are simpler, no null-checking
            if (src.IsValueType)
                return Expression.Block(variables, body.Concat(new[] { instance }));

            var (nullCheck, result, returnLabel) = HelperExtensions.BuildNullCheckedReturn(src, tgt, source, instance);
            return Expression.Block(
                variables,
                new Expression[] { nullCheck }
                    .Concat(body)
                    .Concat(new Expression[] { result, returnLabel })
            );
        };
}
