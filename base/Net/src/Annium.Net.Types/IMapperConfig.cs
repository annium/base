using System;

namespace Annium.Net.Types;

/// <summary>
/// Configuration interface for the type mapper that allows customizing how types are processed during mapping.
/// Provides methods to configure base types, ignored types, included types, excluded types, array types, and record types.
/// </summary>
public interface IMapperConfig
{
    #region base

    /// <summary>
    /// Configures a type to be treated as a base type with the specified name.
    /// Base types are mapped to primitive type names rather than their full .NET type representation.
    /// </summary>
    /// <param name="type">The .NET type to configure as a base type</param>
    /// <param name="name">The name to use for this base type in the mapping output</param>
    /// <returns>The mapper configuration for method chaining</returns>
    IMapperConfig SetBaseType(Type type, string name);

    /// <summary>
    /// Determines whether the specified type is configured as a base type.
    /// </summary>
    /// <param name="type">The type to check</param>
    /// <returns>True if the type is configured as a base type, false otherwise</returns>
    bool IsBaseType(Type type);

    #endregion

    #region ignore

    /// <summary>
    /// Configures types matching the predicate to be ignored during mapping.
    /// Ignored types are not processed or included in the output models.
    /// </summary>
    /// <param name="matcher">A predicate function that determines which types to ignore</param>
    /// <returns>The mapper configuration for method chaining</returns>
    IMapperConfig Ignore(Predicate<Type> matcher);

    /// <summary>
    /// Determines whether the specified type should be ignored during mapping.
    /// </summary>
    /// <param name="type">The type to check</param>
    /// <returns>True if the type should be ignored, false otherwise</returns>
    bool IsIgnored(Type type);

    #endregion

    #region include

    /// <summary>
    /// Explicitly includes a type for processing during mapping.
    /// This can override certain exclusion rules for specific types.
    /// </summary>
    /// <param name="type">The type to explicitly include</param>
    /// <returns>The mapper configuration for method chaining</returns>
    IMapperConfig Include(Type type);

    #endregion

    #region exclude

    /// <summary>
    /// Configures types matching the predicate to be excluded from mapping.
    /// Excluded types are processed but marked as excluded in the output.
    /// </summary>
    /// <param name="matcher">A predicate function that determines which types to exclude</param>
    /// <returns>The mapper configuration for method chaining</returns>
    IMapperConfig Exclude(Predicate<Type> matcher);

    /// <summary>
    /// Determines whether the specified type is excluded from mapping.
    /// </summary>
    /// <param name="type">The type to check</param>
    /// <returns>True if the type is excluded, false otherwise</returns>
    bool IsExcluded(Type type);

    #endregion

    #region array

    /// <summary>
    /// Configures a type to be treated as an array type.
    /// Array types are mapped using special array reference handling.
    /// </summary>
    /// <param name="type">The type to configure as an array type</param>
    /// <returns>The mapper configuration for method chaining</returns>
    IMapperConfig SetArray(Type type);

    /// <summary>
    /// Determines whether the specified type is configured as an array type.
    /// </summary>
    /// <param name="type">The type to check</param>
    /// <returns>True if the type is configured as an array type, false otherwise</returns>
    bool IsArray(Type type);

    #endregion

    #region record

    /// <summary>
    /// Configures a type to be treated as a record type.
    /// Record types are mapped with special handling for immutable data structures.
    /// </summary>
    /// <param name="type">The type to configure as a record type</param>
    /// <returns>The mapper configuration for method chaining</returns>
    IMapperConfig SetRecord(Type type);

    /// <summary>
    /// Determines whether the specified type is configured as a record type.
    /// </summary>
    /// <param name="type">The type to check</param>
    /// <returns>True if the type is configured as a record type, false otherwise</returns>
    bool IsRecord(Type type);

    #endregion
}
