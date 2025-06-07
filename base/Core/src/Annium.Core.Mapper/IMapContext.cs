namespace Annium.Core.Mapper;

/// <summary>
/// Provides context for mapping operations, allowing nested mapping calls during the mapping process
/// </summary>
public interface IMapContext
{
    /// <summary>
    /// Maps a source object to the specified destination type within the current mapping context
    /// </summary>
    /// <typeparam name="T">The destination type</typeparam>
    /// <param name="source">The source object to map</param>
    /// <returns>The mapped object of type T</returns>
    T Map<T>(object source);
}
