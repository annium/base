using System;
using System.Collections.Concurrent;

namespace Annium.Core.Runtime.Types;

/// <summary>
/// Extension methods for Type to provide additional type identification functionality
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    /// Cache for type IDs to avoid repeated creation
    /// </summary>
    private static readonly ConcurrentDictionary<Type, TypeId> _typeIds = new();

    /// <summary>
    /// Gets or creates a TypeId for the specified type
    /// </summary>
    /// <param name="type">The type to get an ID for</param>
    /// <returns>The TypeId for the specified type</returns>
    public static TypeId GetTypeId(this Type type) => _typeIds.GetOrAdd(type, TypeId.Create);

    /// <summary>
    /// Gets the string identifier for the specified type
    /// </summary>
    /// <param name="type">The type to get an ID string for</param>
    /// <returns>The string identifier for the type</returns>
    public static string GetIdString(this Type type) => TypeId.Create(type).Id;
}
