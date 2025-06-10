using System;

namespace Annium.Core.Mapper.Internal;

/// <summary>
/// Context for map resolvers providing access to mapping operations
/// </summary>
public interface IMapResolverContext
{
    /// <summary>
    /// Gets the lazy-initialized map context
    /// </summary>
    Lazy<IMapContext> MapContext { get; }

    /// <summary>
    /// Gets the mapping delegate between the specified types
    /// </summary>
    /// <param name="src">The source type</param>
    /// <param name="tgt">The target type</param>
    /// <returns>The mapping delegate</returns>
    Delegate GetMap(Type src, Type tgt);

    /// <summary>
    /// Resolves a mapping between the specified types
    /// </summary>
    /// <param name="src">The source type</param>
    /// <param name="tgt">The target type</param>
    /// <returns>The resolved mapping</returns>
    Mapping ResolveMapping(Type src, Type tgt);
}
