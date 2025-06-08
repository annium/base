using System;

namespace Annium.Core.Mapper.Internal;

/// <summary>
/// Implementation of map resolver context that provides access to mapping operations for resolvers
/// </summary>
internal class MapResolverContext : IMapResolverContext
{
    /// <summary>
    /// Gets the lazy-initialized map context
    /// </summary>
    public Lazy<IMapContext> MapContext { get; }

    /// <summary>
    /// Function to get mapping delegates
    /// </summary>
    private readonly Func<Type, Type, Delegate> _getMap;

    /// <summary>
    /// Function to resolve mappings
    /// </summary>
    private readonly Func<Type, Type, Mapping> _resolveMapping;

    /// <summary>
    /// Initializes a new instance of the MapResolverContext class
    /// </summary>
    /// <param name="getMap">Function to get mapping delegates</param>
    /// <param name="resolveMapping">Function to resolve mappings</param>
    /// <param name="mapContext">The lazy-initialized map context</param>
    public MapResolverContext(
        Func<Type, Type, Delegate> getMap,
        Func<Type, Type, Mapping> resolveMapping,
        Lazy<IMapContext> mapContext
    )
    {
        MapContext = mapContext;
        _getMap = getMap;
        _resolveMapping = resolveMapping;
    }

    /// <summary>
    /// Gets the mapping delegate between the specified types
    /// </summary>
    /// <param name="src">The source type</param>
    /// <param name="tgt">The target type</param>
    /// <returns>The mapping delegate</returns>
    public Delegate GetMap(Type src, Type tgt) => _getMap(src, tgt);

    /// <summary>
    /// Resolves a mapping between the specified types
    /// </summary>
    /// <param name="src">The source type</param>
    /// <param name="tgt">The target type</param>
    /// <returns>The resolved mapping</returns>
    public Mapping ResolveMapping(Type src, Type tgt) => _resolveMapping(src, tgt);
}
