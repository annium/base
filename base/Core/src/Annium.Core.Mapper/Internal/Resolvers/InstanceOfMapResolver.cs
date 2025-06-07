using System;

namespace Annium.Core.Mapper.Internal.Resolvers;

/// <summary>
/// Map resolver that creates identity mappings when the target type is assignable from the source type
/// </summary>
internal class InstanceOfMapResolver : IMapResolver
{
    /// <summary>
    /// Determines whether this resolver can create a mapping between the specified source and target types
    /// </summary>
    /// <param name="src">The source type</param>
    /// <param name="tgt">The target type</param>
    /// <returns>True if the target type is assignable from the source type, otherwise false</returns>
    public bool CanResolveMap(Type src, Type tgt) => tgt.IsAssignableFrom(src);

    /// <summary>
    /// Resolves and creates an identity mapping that returns the source as-is
    /// </summary>
    /// <param name="src">The source type</param>
    /// <param name="tgt">The target type</param>
    /// <param name="cfg">The mapping configuration</param>
    /// <param name="ctx">The resolver context</param>
    /// <returns>The identity mapping</returns>
    public Mapping ResolveMap(Type src, Type tgt, IMapConfiguration cfg, IMapResolverContext ctx) => source => source;
}
