using System;
using System.Collections.Generic;
using System.Reflection;

namespace Annium.Core.Runtime.Types;

/// <summary>
/// Provides functionality for managing and resolving types in the application.
/// </summary>
/// <remarks>
/// This interface is responsible for type discovery, resolution, and management.
/// It supports various ways of resolving types including by ID, key, signature, and instance.
/// </remarks>
public interface ITypeManager
{
    /// <summary>
    /// Gets a collection of all types managed by this type manager.
    /// </summary>
    /// <remarks>
    /// This collection includes all types that have been discovered and registered
    /// with the type manager.
    /// </remarks>
    IReadOnlyCollection<Type> Types { get; }

    /// <summary>
    /// Checks if there are any implementations of the specified base type.
    /// </summary>
    /// <param name="baseType">The base type to check for implementations.</param>
    /// <returns>True if there are any implementations; otherwise, false.</returns>
    /// <remarks>
    /// Example usage:
    /// <code>
    /// if (typeManager.HasImplementations(typeof(IHandler)))
    /// {
    ///     // Handle implementations
    /// }
    /// </code>
    /// </remarks>
    bool HasImplementations(Type baseType);

    /// <summary>
    /// Gets all implementations of the specified base type.
    /// </summary>
    /// <param name="baseType">The base type to get implementations for.</param>
    /// <returns>An array of types that implement the base type.</returns>
    /// <remarks>
    /// Example usage:
    /// <code>
    /// var handlers = typeManager.GetImplementations(typeof(IHandler));
    /// </code>
    /// </remarks>
    Type[] GetImplementations(Type baseType);

    /// <summary>
    /// Gets the property that is marked as the resolution ID for the specified type.
    /// </summary>
    /// <param name="baseType">The type to get the resolution ID property for.</param>
    /// <returns>The property info for the resolution ID, or null if not found.</returns>
    /// <remarks>
    /// This method looks for properties marked with the ResolutionId attribute.
    /// </remarks>
    PropertyInfo? GetResolutionIdProperty(Type baseType);

    /// <summary>
    /// Gets the property that is marked as the resolution key for the specified type.
    /// </summary>
    /// <param name="baseType">The type to get the resolution key property for.</param>
    /// <returns>The property info for the resolution key, or null if not found.</returns>
    /// <remarks>
    /// This method looks for properties marked with the ResolutionKey attribute.
    /// </remarks>
    PropertyInfo? GetResolutionKeyProperty(Type baseType);

    /// <summary>
    /// Gets the type ID for the specified ID string.
    /// </summary>
    /// <param name="id">The ID string to look up.</param>
    /// <returns>The type ID if found; otherwise, null.</returns>
    /// <remarks>
    /// Example usage:
    /// <code>
    /// var typeId = typeManager.GetTypeId("MyHandler");
    /// </code>
    /// </remarks>
    TypeId? GetTypeId(string id);

    /// <summary>
    /// Resolves a type by its ID.
    /// </summary>
    /// <param name="id">The ID of the type to resolve.</param>
    /// <returns>The resolved type, or null if not found.</returns>
    /// <remarks>
    /// Example usage:
    /// <code>
    /// var type = typeManager.ResolveById("MyHandler");
    /// </code>
    /// </remarks>
    Type? ResolveById(string id);

    /// <summary>
    /// Resolves a type by its key and base type.
    /// </summary>
    /// <param name="key">The key to resolve the type with.</param>
    /// <param name="baseType">The base type of the type to resolve.</param>
    /// <returns>The resolved type, or null if not found.</returns>
    /// <remarks>
    /// Example usage:
    /// <code>
    /// var handler = typeManager.ResolveByKey("key", typeof(IHandler));
    /// </code>
    /// </remarks>
    Type? ResolveByKey(object key, Type baseType);

    /// <summary>
    /// Resolves a type by its signature and base type.
    /// </summary>
    /// <param name="signature">The signature to resolve the type with.</param>
    /// <param name="baseType">The base type of the type to resolve.</param>
    /// <param name="exact">Whether to require an exact match of the signature.</param>
    /// <returns>The resolved type, or null if not found.</returns>
    /// <remarks>
    /// Example usage:
    /// <code>
    /// var handler = typeManager.ResolveBySignature(new[] { "key1", "key2" }, typeof(IHandler));
    /// </code>
    /// </remarks>
    Type? ResolveBySignature(IEnumerable<string> signature, Type baseType, bool exact = false);

    /// <summary>
    /// Resolves a type based on an instance and base type.
    /// </summary>
    /// <param name="instance">The instance to resolve the type for.</param>
    /// <param name="baseType">The base type of the type to resolve.</param>
    /// <returns>The resolved type, or null if not found.</returns>
    /// <remarks>
    /// Example usage:
    /// <code>
    /// var handler = typeManager.Resolve(handlerInstance, typeof(IHandler));
    /// </code>
    /// </remarks>
    Type? Resolve(object instance, Type baseType);
}
