namespace Annium.Core.Mapper.Internal;

/// <summary>
/// Implementation of map context that provides mapping functionality during mapping operations
/// </summary>
internal class MapContext : IMapContext
{
    /// <summary>
    /// The mapper instance used for nested mapping operations
    /// </summary>
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the MapContext class
    /// </summary>
    /// <param name="mapper">The mapper instance to use for nested mappings</param>
    public MapContext(IMapper mapper)
    {
        _mapper = mapper;
    }

    /// <summary>
    /// Maps a source object to the specified destination type within the current mapping context
    /// </summary>
    /// <typeparam name="T">The destination type</typeparam>
    /// <param name="source">The source object to map</param>
    /// <returns>The mapped object of type T</returns>
    public T Map<T>(object source) => _mapper.Map<T>(source);
}
