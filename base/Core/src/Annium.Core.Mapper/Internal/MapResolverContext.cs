using System;

namespace Annium.Core.Mapper.Internal;

internal class MapResolverContext : IMapResolverContext
{
    public Lazy<IMapContext> MapContext { get; }
    private readonly Func<Type, Type, Delegate> _getMap;
    private readonly Func<Type, Type, Mapping> _resolveMapping;

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

    public Delegate GetMap(Type src, Type tgt) => _getMap(src, tgt);

    public Mapping ResolveMapping(Type src, Type tgt) => _resolveMapping(src, tgt);
}