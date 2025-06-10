using System;

namespace Annium.Core.Mapper;

/// <summary>
/// Provides mapping functionality between different types with support for checking mapping availability
/// </summary>
public interface IMapper
{
    /// <summary>
    /// Determines whether a mapping exists from the source object's type to the specified destination type
    /// </summary>
    /// <typeparam name="T">The destination type</typeparam>
    /// <param name="source">The source object</param>
    /// <returns>True if a mapping exists, otherwise false</returns>
    bool HasMap<T>(object? source);

    /// <summary>
    /// Determines whether a mapping exists from the source object's type to the specified destination type
    /// </summary>
    /// <param name="source">The source object</param>
    /// <param name="type">The destination type</param>
    /// <returns>True if a mapping exists, otherwise false</returns>
    bool HasMap(object? source, Type? type);

    /// <summary>
    /// Maps a source object to the specified destination type
    /// </summary>
    /// <typeparam name="T">The destination type</typeparam>
    /// <param name="source">The source object to map</param>
    /// <returns>The mapped object of type T</returns>
    T Map<T>(object? source);

    /// <summary>
    /// Maps a source object to the specified destination type
    /// </summary>
    /// <param name="source">The source object to map</param>
    /// <param name="type">The destination type</param>
    /// <returns>The mapped object of the specified type</returns>
    object? Map(object? source, Type type);
}
