using System;

namespace Annium.Core.Mapper.Internal;

/// <summary>
/// Interface for building and managing mapping configurations
/// </summary>
public interface IMapBuilder
{
    /// <summary>
    /// Adds a profile configured by the provided action
    /// </summary>
    /// <param name="configure">The profile configuration action</param>
    /// <returns>The map builder for method chaining</returns>
    IMapBuilder AddProfile(Action<Profile> configure);

    /// <summary>
    /// Adds a profile of the specified type
    /// </summary>
    /// <typeparam name="T">The profile type</typeparam>
    /// <returns>The map builder for method chaining</returns>
    IMapBuilder AddProfile<T>()
        where T : Profile;

    /// <summary>
    /// Adds a profile of the specified type
    /// </summary>
    /// <param name="profileType">The profile type</param>
    /// <returns>The map builder for method chaining</returns>
    IMapBuilder AddProfile(Type profileType);

    /// <summary>
    /// Determines if a mapping exists between the specified types
    /// </summary>
    /// <param name="src">The source type</param>
    /// <param name="tgt">The target type</param>
    /// <returns>True if a mapping exists, otherwise false</returns>
    bool HasMap(Type src, Type tgt);

    /// <summary>
    /// Gets the mapping delegate between the specified types
    /// </summary>
    /// <param name="src">The source type</param>
    /// <param name="tgt">The target type</param>
    /// <returns>The mapping delegate</returns>
    Delegate GetMap(Type src, Type tgt);
}
