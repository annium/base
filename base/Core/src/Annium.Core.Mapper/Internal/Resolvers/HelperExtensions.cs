using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Annium.Core.Mapper.Internal.Resolvers;

/// <summary>
/// Helper extension methods for type inspection and property access
/// </summary>
internal static class HelperExtensions
{
    /// <summary>
    /// Binding flags for reflecting over all instance members (public and non-public) of a type.
    /// </summary>
    private const BindingFlags AllInstance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

    /// <summary>
    /// Gets the element type of an enumerable type
    /// </summary>
    /// <param name="type">The type to inspect</param>
    /// <returns>The element type if the type is enumerable, otherwise null</returns>
    public static Type? GetEnumerableElementType(this Type type)
    {
        if (type.IsArray)
            return type.GetElementType();

        if (type.GenericTypeArguments.Length == 0)
            return null;

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            return type.GenericTypeArguments[0];

        var enumerable = type.GetTypeInfo()
            .ImplementedInterfaces.FirstOrDefault(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)
            );

        return enumerable?.GenericTypeArguments[0];
    }

    /// <summary>
    /// Gets the constructor with the most parameters
    /// </summary>
    /// <param name="type">The type to inspect</param>
    /// <returns>The constructor with the most parameters</returns>
    public static ConstructorInfo GetParametrizedConstructor(this Type type) =>
        type.GetConstructors(AllInstance)
            .Where(x => x.GetParameters().Length > 0)
            .OrderByDescending(c => c.GetParameters().Length)
            .FirstOrDefault()
        ?? throw new InvalidOperationException("Parameterized constructor not found");

    /// <summary>
    /// Gets the default parameterless constructor
    /// </summary>
    /// <param name="type">The type to inspect</param>
    /// <returns>The default constructor</returns>
    public static ConstructorInfo GetDefaultConstructor(this Type type) =>
        type.GetConstructor(Type.EmptyTypes)
        ?? throw new InvalidOperationException("Parameterless constructor not found");

    /// <summary>
    /// Gets all readable properties of the type
    /// </summary>
    /// <param name="type">The type to inspect</param>
    /// <returns>Array of readable properties</returns>
    public static PropertyInfo[] GetReadableProperties(this Type type) =>
        type.GetProperties(AllInstance).Where(x => x.CanRead).ToArray();

    /// <summary>
    /// Gets all writeable properties of the type
    /// </summary>
    /// <param name="type">The type to inspect</param>
    /// <returns>Array of writeable properties</returns>
    public static PropertyInfo[] GetWriteableProperties(this Type type) =>
        type.GetProperties(AllInstance).Where(x => x.CanWrite).ToArray();

    /// <summary>
    /// Filters writeable target properties down to those eligible for basic assignment: drops members the
    /// configuration maps explicitly or ignores (matched by PropertyType + Name so inherited members reflected
    /// from a derived vs. base type still match) and drops explicit interface implementations (dotted names).
    /// </summary>
    /// <param name="cfg">Mapping configuration supplying the mapped + ignored member sets.</param>
    /// <param name="targets">Candidate writeable target properties.</param>
    /// <returns>The targets remaining for auto-assignment.</returns>
    public static PropertyInfo[] FilterAutoAssignTargets(IMapConfiguration cfg, PropertyInfo[] targets)
    {
        var excludedMembers = cfg.MemberMaps.Keys.Concat(cfg.IgnoredMembers).ToArray();
        return targets
            .Where(target => !excludedMembers.Any(x => x.PropertyType == target.PropertyType && x.Name == target.Name))
            // ignore interface implementations
            .Where(x => !x.Name.Contains('.'))
            .ToArray();
    }

    /// <summary>
    /// Resolves the <c>TryGetValue</c> method on a dictionary source type, throwing a
    /// <see cref="MappingException"/> (rather than a bare null) when it is somehow absent.
    /// </summary>
    /// <param name="src">The dictionary source type.</param>
    /// <param name="tgt">The target type (for the exception message).</param>
    /// <returns>The resolved <c>TryGetValue</c> method.</returns>
    public static MethodInfo ResolveTryGetValue(Type src, Type tgt) =>
        src.GetMethod(nameof(Dictionary<,>.TryGetValue))
        ?? throw new MappingException(
            src,
            tgt,
            $"Failed to resolve method {src.FriendlyName()}.{nameof(Dictionary<,>.TryGetValue)}"
        );

    /// <summary>
    /// Determines whether the type is one of the three string→object dictionary shapes the
    /// dictionary-source resolvers accept (<see cref="Dictionary{TKey,TValue}"/>,
    /// <see cref="IDictionary{TKey,TValue}"/>, <see cref="IReadOnlyDictionary{TKey,TValue}"/>).
    /// </summary>
    /// <param name="type">The candidate source type.</param>
    /// <returns>True if <paramref name="type"/> is a string→object dictionary shape.</returns>
    public static bool IsStringObjectDictionary(this Type type) =>
        type == typeof(Dictionary<string, object>)
        || type == typeof(IDictionary<string, object>)
        || type == typeof(IReadOnlyDictionary<string, object>);

    /// <summary>
    /// Builds the assignment expressions for every configured member mapping in <paramref name="cfg"/>
    /// and appends them to <paramref name="body"/>. Member maps grouped by the same lambda are
    /// evaluated once into a shared variable (added to <paramref name="variables"/>) and then
    /// fan-projected onto each target property — matching how the lambda's source expression carries
    /// multiple properties of the result anonymous type.
    /// </summary>
    /// <param name="cfg">Mapping configuration whose <see cref="IMapConfiguration.MemberMaps"/> drives the emission.</param>
    /// <param name="ctx">Resolver context used to materialise the lambda via <see cref="IMapResolverContext.MapContext"/>.</param>
    /// <param name="repacker">Repacker that rewrites the lambda body so it can run against the resolver-local source expression.</param>
    /// <param name="source">Expression carrying the source value the repacked lambda receives.</param>
    /// <param name="instance">Expression carrying the target instance whose properties get assigned.</param>
    /// <param name="variables">Mutable variable list extended with the shared variables created for multi-member groups.</param>
    /// <param name="body">Mutable expression list extended with the emitted assignments.</param>
    public static void AppendMemberMapAssignments(
        IMapConfiguration cfg,
        IMapResolverContext ctx,
        IRepacker repacker,
        Expression source,
        Expression instance,
        List<ParameterExpression> variables,
        List<Expression> body
    )
    {
        foreach (var group in cfg.MemberMaps.GroupBy(x => x.Value))
        {
            var map = group.Key(ctx.MapContext.Value);
            var members = group.Select(x => x.Key).ToArray();

            if (members.Length == 1)
                body.Add(
                    Expression.Assign(
                        Expression.Property(instance, members.Single()),
                        repacker.Repack(map.Body)(source)
                    )
                );
            else
            {
                var variable = Expression.Variable(map.Body.Type);
                variables.Add(variable);
                body.Add(Expression.Assign(variable, repacker.Repack(map.Body)(source)));

                foreach (var member in members)
                    body.Add(
                        Expression.Assign(
                            Expression.Property(instance, member),
                            Expression.Property(variable, map.Body.Type, member.Name)
                        )
                    );
            }
        }
    }

    /// <summary>
    /// Builds the variable-bound member-mapping expressions for the constructor-style resolvers and
    /// indexes them in <paramref name="mappedMemberVars"/> by lower-cased target property name so the
    /// constructor-parameter selector can look them up. Each grouped lambda evaluates once into a
    /// shared variable; per-member projections then capture each target property's value into its
    /// own typed variable (added to <paramref name="variables"/>).
    /// </summary>
    /// <param name="cfg">Mapping configuration whose <see cref="IMapConfiguration.MemberMaps"/> drives the emission.</param>
    /// <param name="ctx">Resolver context used to materialise the lambda via <see cref="IMapResolverContext.MapContext"/>.</param>
    /// <param name="repacker">Repacker that rewrites the lambda body so it can run against the resolver-local source expression.</param>
    /// <param name="source">Expression carrying the source value the repacked lambda receives.</param>
    /// <param name="variables">Mutable variable list extended with every per-member variable created.</param>
    /// <param name="body">Mutable expression list extended with the variable assignments.</param>
    /// <param name="mappedMemberVars">Lookup populated with (lower-case-target-name → variable expression) entries.</param>
    public static void AppendMemberMapVariables(
        IMapConfiguration cfg,
        IMapResolverContext ctx,
        IRepacker repacker,
        Expression source,
        List<ParameterExpression> variables,
        List<Expression> body,
        Dictionary<string, ParameterExpression> mappedMemberVars
    )
    {
        foreach (var group in cfg.MemberMaps.GroupBy(x => x.Value))
        {
            var map = group.Key(ctx.MapContext.Value);
            var members = group.Select(x => x.Key).ToArray();

            if (members.Length == 1)
            {
                var member = members.Single();
                var memberVar = Expression.Variable(member.PropertyType);
                variables.Add(memberVar);
                body.Add(Expression.Assign(memberVar, repacker.Repack(map.Body)(source)));
                mappedMemberVars[member.Name.ToLowerInvariant()] = memberVar;
            }
            else
            {
                var resultVar = Expression.Variable(map.Body.Type);
                variables.Add(resultVar);
                body.Add(Expression.Assign(resultVar, repacker.Repack(map.Body)(source)));

                foreach (var member in members)
                {
                    var memberVar = Expression.Variable(member.PropertyType);
                    variables.Add(memberVar);
                    body.Add(Expression.Assign(memberVar, Expression.Property(resultVar, map.Body.Type, member.Name)));
                    mappedMemberVars[member.Name.ToLowerInvariant()] = memberVar;
                }
            }
        }
    }

    /// <summary>
    /// Builds the labelled early-return scaffolding every reference-type resolver needs: a return-label
    /// keyed to <paramref name="tgt"/>, a null guard that exits with <c>default(tgt)</c> when
    /// <paramref name="source"/> equals <c>default(src)</c>, the success-path return that yields
    /// <paramref name="instance"/>, and the label statement that terminates the block.
    /// </summary>
    /// <param name="src">Source type used for the null guard comparison.</param>
    /// <param name="tgt">Target type used to type the label and default value.</param>
    /// <param name="source">Expression carrying the source value to null-check.</param>
    /// <param name="instance">Expression carrying the built target instance to return on the success path.</param>
    /// <returns>The three expression fragments callers splice around <c>body</c>.</returns>
    public static (Expression NullCheck, Expression Result, LabelExpression ReturnLabel) BuildNullCheckedReturn(
        Type src,
        Type tgt,
        Expression source,
        Expression instance
    )
    {
        var returnTarget = Expression.Label(tgt);
        var defaultValue = Expression.Default(tgt);
        var returnExpression = Expression.Return(returnTarget, defaultValue, tgt);
        var returnLabel = Expression.Label(returnTarget, defaultValue);
        var nullCheck = Expression.IfThen(Expression.Equal(source, Expression.Default(src)), returnExpression);
        var result = Expression.Return(returnTarget, instance, tgt);
        return (nullCheck, result, returnLabel);
    }
}
