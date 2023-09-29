namespace Annium.Core.Mapper.Internal;

internal class MapContext : IMapContext
{
    private readonly IMapper _mapper;

    public MapContext(
        IMapper mapper
    )
    {
        _mapper = mapper;
    }

    public T Map<T>(object source) => _mapper.Map<T>(source);
}