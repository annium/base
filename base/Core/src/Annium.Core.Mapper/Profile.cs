using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Annium.Core.Mapper.Internal;
using Annium.Reflection;

namespace Annium.Core.Mapper;

/// <summary>
/// Base class for defining mapping configurations between types
/// </summary>
public abstract class Profile
{
    /// <summary>
    /// Gets the map configurations defined in this profile
    /// </summary>
    internal IReadOnlyDictionary<ValueTuple<Type, Type>, IMapConfiguration> MapConfigurations => _mapConfigurations;

    /// <summary>
    /// The mapping configurations defined in this profile
    /// </summary>
    private readonly Dictionary<ValueTuple<Type, Type>, IMapConfiguration> _mapConfigurations = new();

    /// <summary>
    /// Defines a mapping from source type to target type using an expression
    /// </summary>
    /// <typeparam name="TSource">The source type</typeparam>
    /// <typeparam name="TTarget">The target type</typeparam>
    /// <param name="mapping">The mapping expression</param>
    public void Map<TSource, TTarget>(Expression<Func<TSource, TTarget>> mapping)
    {
        var map = Map<TSource, TTarget>();

        map.With(mapping);
    }

    /// <summary>
    /// Defines a contextual mapping from source type to target type using a factory function
    /// </summary>
    /// <typeparam name="TSource">The source type</typeparam>
    /// <typeparam name="TTarget">The target type</typeparam>
    /// <param name="mapping">The contextual mapping expression factory</param>
    public void Map<TSource, TTarget>(Func<IMapContext, Expression<Func<TSource, TTarget>>> mapping)
    {
        var map = Map<TSource, TTarget>();

        map.With(mapping);
    }

    /// <summary>
    /// Gets a configuration builder for mapping from source type to target type
    /// </summary>
    /// <typeparam name="TSource">The source type</typeparam>
    /// <typeparam name="TTarget">The target type</typeparam>
    /// <returns>The configuration builder</returns>
    public IMapConfigurationBuilder<TSource, TTarget> Map<TSource, TTarget>()
    {
        // Last-wins: a re-registration replaces the prior builder. Intentional — built-in profiles
        // (e.g. DefaultProfile) declare numeric conversions symmetrically in both directions' register
        // methods and rely on the replacement to coalesce. NOTE for callers: a downstream `.With(...)`
        // on the previous builder is lost; chain `.For(...)/.Ignore(...)/.With(...)` directly off the
        // single Map<TS,TD>() call rather than calling Map<TS,TD>() twice.
        var map = new MapConfigurationBuilder<TSource, TTarget>();
        _mapConfigurations[(typeof(TSource), typeof(TTarget))] = map.Result;

        return map;
    }
}
