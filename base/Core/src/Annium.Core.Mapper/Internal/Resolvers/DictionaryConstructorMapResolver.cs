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
    public bool CanResolveMap(Type src, Type tgt)
    {
        return (
                src == typeof(Dictionary<string, object>)
                || src == typeof(IDictionary<string, object>)
                || src == typeof(IReadOnlyDictionary<string, object>)
            ) && tgt.GetConstructor(Type.EmptyTypes) is null;
    }

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
            var tryGetValue =
                src.GetMethod(nameof(Dictionary<object, object>.TryGetValue))
                ?? throw new MappingException(
                    src,
                    tgt,
                    $"Failed to resolve method {src.FriendlyName()}.{nameof(Dictionary<object, object>.TryGetValue)}"
                );
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
                    var item = Expression.Condition(
                        Expression.Call(source, tryGetValue, Expression.Constant(paramName), itemVar),
                        itemVar,
                        Expression.Throw(
                            Expression.New(
                                typeof(KeyNotFoundException).GetConstructor(new[] { typeof(string) })!,
                                Expression.Constant($"Missing value for property '{paramName}'")
                            )
                        )
                    );

                    return map(item);
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
