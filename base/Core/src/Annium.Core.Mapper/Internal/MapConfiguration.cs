using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Annium.Core.Mapper.Internal;

/// <summary>
/// Implementation of mapping configuration that stores mapping expressions and ignored members
/// </summary>
internal class MapConfiguration : IMapConfiguration
{
    /// <summary>
    /// Gets an empty mapping configuration instance
    /// </summary>
    public static IMapConfiguration Empty { get; } = new MapConfiguration();

    /// <summary>
    /// Gets the global mapping function
    /// </summary>
    public Func<IMapContext, LambdaExpression>? MapWith { get; private set; }

    /// <summary>
    /// Gets the member-specific mapping functions
    /// </summary>
    public IReadOnlyDictionary<PropertyInfo, Func<IMapContext, LambdaExpression>> MemberMaps => _memberMaps;

    /// <summary>
    /// Gets the collection of ignored members
    /// </summary>
    public IReadOnlyCollection<PropertyInfo> IgnoredMembers => _ignoredMembers;

    /// <summary>
    /// Member-specific mapping functions
    /// </summary>
    private readonly Dictionary<PropertyInfo, Func<IMapContext, LambdaExpression>> _memberMaps = new();

    /// <summary>
    /// Set of ignored members
    /// </summary>
    private readonly HashSet<PropertyInfo> _ignoredMembers = new();

    /// <summary>
    /// Sets the global mapping expression
    /// </summary>
    /// <param name="mapWith">The mapping expression</param>
    public void SetMapWith(LambdaExpression mapWith)
    {
        MapWith = _ => mapWith;
    }

    /// <summary>
    /// Sets the global mapping expression factory
    /// </summary>
    /// <param name="mapWith">The mapping expression factory</param>
    public void SetMapWith(Func<IMapContext, LambdaExpression> mapWith)
    {
        MapWith = mapWith;
    }

    /// <summary>
    /// Adds member-specific mapping expressions for the specified properties
    /// </summary>
    /// <param name="properties">The properties to configure</param>
    /// <param name="mapWith">The mapping expression</param>
    public void AddMapWithFor(IReadOnlyCollection<PropertyInfo> properties, LambdaExpression mapWith)
    {
        foreach (var property in properties)
            _memberMaps[property] = _ => mapWith;
    }

    /// <summary>
    /// Adds member-specific mapping expression factories for the specified properties
    /// </summary>
    /// <param name="properties">The properties to configure</param>
    /// <param name="mapWith">The mapping expression factory</param>
    public void AddMapWithFor(IReadOnlyCollection<PropertyInfo> properties, Func<IMapContext, LambdaExpression> mapWith)
    {
        foreach (var property in properties)
            _memberMaps[property] = mapWith;
    }

    /// <summary>
    /// Adds properties to the ignored members collection
    /// </summary>
    /// <param name="properties">The properties to ignore</param>
    public void Ignore(IReadOnlyCollection<PropertyInfo> properties)
    {
        foreach (var property in properties)
            _ignoredMembers.Add(property);
    }
}
