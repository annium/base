using System;
using System.Reflection;

namespace Annium.Core.Mapper.Internal;

/// <summary>
/// Implementation of mapper that provides mapping functionality between different types
/// </summary>
internal class Mapper : IMapper
{
    /// <summary>
    /// The map builder used for resolving and building mappings
    /// </summary>
    private readonly IMapBuilder _mapBuilder;

    /// <summary>
    /// Initializes a new instance of the Mapper class
    /// </summary>
    /// <param name="mapBuilder">The map builder for resolving mappings</param>
    public Mapper(IMapBuilder mapBuilder)
    {
        _mapBuilder = mapBuilder;
    }

    /// <summary>
    /// Determines whether a mapping exists from the source object's type to the specified destination type
    /// </summary>
    /// <typeparam name="T">The destination type</typeparam>
    /// <param name="source">The source object</param>
    /// <returns>True if a mapping exists, otherwise false</returns>
    public bool HasMap<T>(object? source) => HasMap(source, typeof(T));

    /// <summary>
    /// Determines whether a mapping exists from the source object's type to the specified destination type
    /// </summary>
    /// <param name="source">The source object</param>
    /// <param name="type">The destination type</param>
    /// <returns>True if a mapping exists, otherwise false</returns>
    public bool HasMap(object? source, Type? type)
    {
        if (source is null || type is null)
            return false;

        if (type.IsInstanceOfType(source))
            return true;

        return _mapBuilder.HasMap(source.GetType(), type);
    }

    /// <summary>
    /// Maps a source object to the specified destination type.
    /// </summary>
    /// <typeparam name="T">The destination type.</typeparam>
    /// <param name="source">The source object to map.</param>
    /// <returns>The mapped object of type T.</returns>
    public T Map<T>(object source) => (T)Map(source, typeof(T));

    /// <summary>
    /// Maps a source object to the specified destination type.
    /// </summary>
    /// <param name="source">The source object to map.</param>
    /// <param name="type">The destination type.</param>
    /// <returns>The mapped object of the specified type.</returns>
    public object Map(object source, Type type)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(type);

        var map = _mapBuilder.GetMap(source.GetType(), type);

        try
        {
            // compiled mapping delegates always return a non-null instance of the target type
            return map.DynamicInvoke(source).NotNull();
        }
        catch (TargetInvocationException ex)
        {
            // unwrap to surface the user-visible exception; fall back to the wrapper if InnerException is somehow absent
            throw ex.InnerException ?? ex;
        }
    }
}
