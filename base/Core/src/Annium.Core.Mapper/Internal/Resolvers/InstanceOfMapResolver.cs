using System;

namespace Annium.Core.Mapper.Internal.Resolvers;

internal class InstanceOfMapResolver : IMapResolver
{
    public bool CanResolveMap(Type src, Type tgt) => tgt.IsAssignableFrom(src);

    public Mapping ResolveMap(Type src, Type tgt, IMapConfiguration cfg, IMapResolverContext ctx) => source => source;
}