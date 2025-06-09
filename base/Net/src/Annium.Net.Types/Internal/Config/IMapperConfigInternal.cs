using System;
using System.Collections.Generic;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Config;

/// <summary>
/// Internal interface extending mapper configuration with additional functionality for runtime operations.
/// Provides contextual type checking and base type reference resolution.
/// </summary>
internal interface IMapperConfigInternal : IMapperConfig
{
    /// <summary>
    /// Gets all types that have been explicitly included for processing.
    /// </summary>
    IReadOnlyCollection<Type> Included { get; }

    /// <summary>
    /// Gets the base type reference for the specified type, if configured as a base type.
    /// </summary>
    /// <param name="type">The type to get the base type reference for</param>
    /// <returns>The base type reference, or null if not configured as a base type</returns>
    BaseTypeRef? GetBaseTypeRefFor(Type type);

    /// <summary>
    /// Determines whether the specified contextual type should be ignored during mapping.
    /// </summary>
    /// <param name="type">The contextual type to check</param>
    /// <returns>True if the type should be ignored, false otherwise</returns>
    bool IsIgnored(ContextualType type);

    /// <summary>
    /// Determines whether the specified contextual type is excluded from mapping.
    /// </summary>
    /// <param name="type">The contextual type to check</param>
    /// <returns>True if the type is excluded, false otherwise</returns>
    bool IsExcluded(ContextualType type);

    /// <summary>
    /// Determines whether the specified contextual type should be treated as an array.
    /// </summary>
    /// <param name="type">The contextual type to check</param>
    /// <returns>True if the type should be treated as an array, false otherwise</returns>
    bool IsArray(ContextualType type);

    /// <summary>
    /// Determines whether the specified contextual type should be treated as a record.
    /// </summary>
    /// <param name="type">The contextual type to check</param>
    /// <returns>True if the type should be treated as a record, false otherwise</returns>
    bool IsRecord(ContextualType type);
}
