using System;
using Annium.Core.Mapper.Internal;

namespace Annium.Core.Mapper;

public interface IMapResolver
{
    bool CanResolveMap(Type src, Type tgt);
    Mapping ResolveMap(Type src, Type tgt, IMapConfiguration cfg, IMapResolverContext ctx);
}