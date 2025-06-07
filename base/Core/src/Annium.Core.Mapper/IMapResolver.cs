using System;
using Annium.Core.Mapper.Internal;

namespace Annium.Core.Mapper;

/// <summary>
/// Resolves and creates mapping configurations between source and target types
/// </summary>
public interface IMapResolver
{
    /// <summary>
    /// Determines whether this resolver can create a mapping between the specified source and target types
    /// </summary>
    /// <param name="src">The source type</param>
    /// <param name="tgt">The target type</param>
    /// <returns>True if the resolver can create a mapping, otherwise false</returns>
    bool CanResolveMap(Type src, Type tgt);

    /// <summary>
    /// Resolves and creates a mapping between the specified source and target types
    /// </summary>
    /// <param name="src">The source type</param>
    /// <param name="tgt">The target type</param>
    /// <param name="cfg">The mapping configuration</param>
    /// <param name="ctx">The resolver context</param>
    /// <returns>The resolved mapping</returns>
    Mapping ResolveMap(Type src, Type tgt, IMapConfiguration cfg, IMapResolverContext ctx);
}
