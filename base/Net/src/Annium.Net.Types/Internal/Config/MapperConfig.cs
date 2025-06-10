using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Net.Types.Internal.Extensions;
using Annium.Net.Types.Refs;
using Annium.Reflection.Types;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Config;

/// <summary>
/// Internal implementation of mapper configuration that manages type processing rules and settings.
/// Handles base types, ignored types, excluded types, arrays, and records configuration.
/// </summary>
internal class MapperConfig : IMapperConfigInternal
{
    #region base

    /// <summary>
    /// Dictionary that maps types to their base type references for type mapping
    /// </summary>
    private readonly Dictionary<Type, BaseTypeRef> _baseTypes = new();

    /// <summary>
    /// Registers a type as a base type with the specified name for type mapping.
    /// </summary>
    /// <param name="type">The type to register as a base type</param>
    /// <param name="name">The name to assign to the base type</param>
    /// <returns>The mapper configuration for method chaining</returns>
    public IMapperConfig SetBaseType(Type type, string name)
    {
        if (type is { IsClass: false, IsValueType: false })
            throw new ArgumentException($"Type {type.FriendlyName()} is neither class nor struct");

        if (type.IsGenericType || type.IsGenericTypeDefinition)
            throw new ArgumentException($"Type {type.FriendlyName()} is generic type");

        if (type.IsGenericTypeParameter)
            throw new ArgumentException($"Type {type.FriendlyName()} is generic type parameter");

        if (!_baseTypes.TryAdd(type, new BaseTypeRef(name)))
            throw new ArgumentException($"Type {type.FriendlyName()} is already registered");

        return this;
    }

    /// <summary>
    /// Determines whether the specified type is registered as a base type.
    /// </summary>
    /// <param name="type">The type to check</param>
    /// <returns>True if the type is a registered base type, false otherwise</returns>
    public bool IsBaseType(Type type) => _baseTypes.GetValueOrDefault(type) is not null;

    /// <summary>
    /// Gets the base type reference for the specified type if it is registered as a base type.
    /// </summary>
    /// <param name="type">The type to get the base type reference for</param>
    /// <returns>The base type reference if found, null otherwise</returns>
    public BaseTypeRef? GetBaseTypeRefFor(Type type) => _baseTypes.GetValueOrDefault(type);

    #endregion

    #region ignored

    /// <summary>
    /// List of predicates that determine which types should be ignored during mapping
    /// </summary>
    private readonly List<Predicate<Type>> _ignored = new();

    /// <summary>
    /// Adds a predicate to ignore types that match the specified condition during mapping.
    /// </summary>
    /// <param name="matcher">A predicate that determines which types to ignore</param>
    /// <returns>The mapper configuration for method chaining</returns>
    public IMapperConfig Ignore(Predicate<Type> matcher)
    {
        _ignored.Add(matcher);

        return this;
    }

    /// <summary>
    /// Determines whether the specified type should be ignored during mapping.
    /// </summary>
    /// <param name="type">The type to check</param>
    /// <returns>True if the type should be ignored, false otherwise</returns>
    public bool IsIgnored(Type type)
    {
        var pure = type.GetPure();

        return _ignored.Any(match => match(pure));
    }

    /// <summary>
    /// Determines whether the specified contextual type should be ignored during mapping.
    /// </summary>
    /// <param name="type">The contextual type to check</param>
    /// <returns>True if the type should be ignored, false otherwise</returns>
    public bool IsIgnored(ContextualType type) => IsIgnored(type.Type);

    #endregion

    #region include

    /// <summary>
    /// Gets the collection of types that have been explicitly included for mapping.
    /// </summary>
    public IReadOnlyCollection<Type> Included => _included;

    /// <summary>
    /// Set of types that have been explicitly included for mapping processing
    /// </summary>
    private readonly HashSet<Type> _included = new();

    /// <summary>
    /// Explicitly includes a type for mapping processing.
    /// </summary>
    /// <param name="type">The type to include</param>
    /// <returns>The mapper configuration for method chaining</returns>
    public IMapperConfig Include(Type type)
    {
        if (type != type.TryGetPure())
            throw new ArgumentException($"Can't register type {type.FriendlyName()} as included");

        if (!_included.Add(type))
            throw new ArgumentException($"Type {type.FriendlyName()} is already registered as included");

        return this;
    }

    #endregion

    #region excluded

    /// <summary>
    /// List of predicates that determine which types should be excluded from mapping
    /// </summary>
    private readonly List<Predicate<Type>> _excluded = new();

    /// <summary>
    /// Adds a predicate to exclude types that match the specified condition from mapping.
    /// </summary>
    /// <param name="matcher">A predicate that determines which types to exclude</param>
    /// <returns>The mapper configuration for method chaining</returns>
    public IMapperConfig Exclude(Predicate<Type> matcher)
    {
        _excluded.Add(matcher);

        return this;
    }

    /// <summary>
    /// Determines whether the specified type should be excluded from mapping.
    /// </summary>
    /// <param name="type">The type to check</param>
    /// <returns>True if the type should be excluded, false otherwise</returns>
    public bool IsExcluded(Type type)
    {
        var pure = type.GetPure();

        return _excluded.Any(match => match(pure));
    }

    /// <summary>
    /// Determines whether the specified contextual type should be excluded from mapping.
    /// </summary>
    /// <param name="type">The contextual type to check</param>
    /// <returns>True if the type should be excluded, false otherwise</returns>
    public bool IsExcluded(ContextualType type) => IsExcluded(type.Type);

    #endregion

    #region array

    /// <summary>
    /// The base array type used for identifying array-like types.
    /// </summary>
    internal static readonly Type BaseArrayType = typeof(IEnumerable<>);

    /// <summary>
    /// Set of types that have been explicitly registered as array types for special handling
    /// </summary>
    private readonly HashSet<Type> _arrayTypes = new();

    /// <summary>
    /// Registers a type as an array type for special handling during mapping.
    /// </summary>
    /// <param name="type">The type to register as an array type</param>
    /// <returns>The mapper configuration for method chaining</returns>
    public IMapperConfig SetArray(Type type)
    {
        if (type != type.TryGetPure())
            throw new ArgumentException($"Can't register type {type.FriendlyName()} as array type");

        if (!type.IsDerivedFrom(BaseArrayType, self: true))
            throw new ArgumentException($"Type {type.FriendlyName()} doesn't implement {BaseArrayType.FriendlyName()}");

        if (!_arrayTypes.Add(type))
            throw new ArgumentException($"Type {type.FriendlyName()} is already registered as array type");

        return this;
    }

    /// <summary>
    /// Determines whether the specified type should be treated as an array during mapping.
    /// </summary>
    /// <param name="type">The type to check</param>
    /// <returns>True if the type should be treated as an array, false otherwise</returns>
    public bool IsArray(Type type)
    {
        return type.IsArray || _arrayTypes.Contains(type.GetPure());
    }

    /// <summary>
    /// Determines whether the specified contextual type should be treated as an array during mapping.
    /// </summary>
    /// <param name="type">The contextual type to check</param>
    /// <returns>True if the type should be treated as an array, false otherwise</returns>
    public bool IsArray(ContextualType type) => IsArray(type.Type);

    #endregion

    #region record

    /// <summary>
    /// The base record value type used for identifying record-like types.
    /// </summary>
    internal static readonly Type BaseRecordValueType = typeof(KeyValuePair<,>);

    /// <summary>
    /// The base record type used for identifying record-like collection types
    /// </summary>
    private static readonly Type _baseRecordType = typeof(IEnumerable<>).MakeGenericType(BaseRecordValueType);

    /// <summary>
    /// Set of types that have been explicitly registered as record types for special handling
    /// </summary>
    private readonly HashSet<Type> _recordTypes = new();

    /// <summary>
    /// Registers a type as a record type for special handling during mapping.
    /// </summary>
    /// <param name="type">The type to register as a record type</param>
    /// <returns>The mapper configuration for method chaining</returns>
    public IMapperConfig SetRecord(Type type)
    {
        if (type != type.TryGetPure())
            throw new ArgumentException($"Can't register type {type.FriendlyName()} as Record type");

        var arrayImplementation = type.GetInterfaces()
            .SingleOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == BaseArrayType);
        if (
            arrayImplementation is null
            || arrayImplementation.GetGenericArguments()[0].GetGenericTypeDefinition() != BaseRecordValueType
        )
            throw new ArgumentException(
                $"Type {type.FriendlyName()} doesn't implement {_baseRecordType.FriendlyName()}"
            );

        if (!_recordTypes.Add(type))
            throw new ArgumentException($"Type {type.FriendlyName()} is already registered as Record type");

        return this;
    }

    /// <summary>
    /// Determines whether the specified type should be treated as a record during mapping.
    /// </summary>
    /// <param name="type">The type to check</param>
    /// <returns>True if the type should be treated as a record, false otherwise</returns>
    public bool IsRecord(Type type)
    {
        return _recordTypes.Contains(type.GetPure());
    }

    /// <summary>
    /// Determines whether the specified contextual type should be treated as a record during mapping.
    /// </summary>
    /// <param name="type">The contextual type to check</param>
    /// <returns>True if the type should be treated as a record, false otherwise</returns>
    public bool IsRecord(ContextualType type) => IsRecord(type.Type);

    #endregion
}
