using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Annium.Core.Runtime.Types;
using Annium.Logging;

namespace Annium.Core.Runtime.Internal.Types;

/// <summary>
/// Internal implementation of type manager that handles type resolution and hierarchy management
/// </summary>
internal class TypeManagerInstance : ITypeManager, ILogSubject
{
    /// <summary>
    /// The logger for this type manager instance
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Contains collection of all types, collected for given Assembly
    /// </summary>
    public IReadOnlyCollection<Type> Types { get; }

    /// <summary>
    /// The type hierarchy mapping ancestors to their descendants
    /// </summary>
    private readonly IReadOnlyDictionary<Ancestor, IReadOnlyCollection<Descendant>> _hierarchy;

    /// <summary>
    /// Mapping of type IDs to their corresponding types
    /// </summary>
    private readonly IReadOnlyDictionary<TypeId, Type> _ids;

    /// <summary>
    /// Initializes a new instance of TypeManagerInstance for the specified assembly
    /// </summary>
    /// <param name="assembly">The assembly to collect types from</param>
    /// <param name="logger">The logger to use for tracing operations</param>
    public TypeManagerInstance(Assembly assembly, ILogger logger)
    {
        Logger = logger;
        this.Trace("start for {assembly}", assembly);
        this.Trace("collect assemblies");
        var assemblies = new AssembliesCollector(logger).Collect(assembly);
        this.Trace("collect types");
        var types = new TypesCollector(logger).Collect(assemblies);
        this.Trace("build hierarchy");
        _hierarchy = HierarchyBuilder.BuildHierarchy(types);
        this.Trace("register {typesCount} ids", types.Count);
        var ids = new Dictionary<TypeId, Type>();
        foreach (var type in types)
            ids[type.GetTypeId()] = type;

        _ids = ids;
        Types = types;
        this.Trace("done");
    }

    /// <summary>
    /// Returns whether given type is registered with one or more of subtypes.
    /// </summary>
    /// <param name="baseType">The base type to check for implementations</param>
    /// <returns>True if the type has implementations; otherwise, false</returns>
    public bool HasImplementations(Type baseType)
    {
        if (baseType is null)
            throw new ArgumentNullException(nameof(baseType));

        return GetImplementations(baseType).Length > 0;
    }

    /// <summary>
    /// Returns all direct implementations of base type
    /// </summary>
    /// <param name="baseType">The base type to get implementations for</param>
    /// <returns>Array of types that implement the base type</returns>
    public Type[] GetImplementations(Type baseType)
    {
        if (baseType is null)
            throw new ArgumentNullException(nameof(baseType));

        return GetImplementationDescendants(baseType).Select(x => x.Type).ToArray();
    }

    /// <summary>
    /// Returns resolution id property for given base type, if exists
    /// </summary>
    /// <param name="baseType">The base type to get the resolution ID property for</param>
    /// <returns>The PropertyInfo for the resolution ID property, or null if not found</returns>
    /// <exception cref="ArgumentNullException">Thrown when baseType is null</exception>
    public PropertyInfo? GetResolutionIdProperty(Type baseType)
    {
        if (baseType is null)
            throw new ArgumentNullException(nameof(baseType));

        var lookupType = baseType.IsGenericType ? baseType.GetGenericTypeDefinition() : baseType;

        var property = _hierarchy.Keys.FirstOrDefault(x => x.Type == lookupType)?.IdProperty;
        if (property is not null && property.PropertyType != typeof(string))
            throw new InvalidOperationException(
                $"Type '{baseType}' id property '{property}' must be of type '{typeof(string)}'"
            );

        return property;
    }

    /// <summary>
    /// Returns resolution key property for given base type, if exists
    /// </summary>
    /// <param name="baseType">The base type to get the resolution key property for</param>
    /// <returns>The PropertyInfo for the resolution key property, or null if not found</returns>
    /// <exception cref="ArgumentNullException">Thrown when baseType is null</exception>
    public PropertyInfo? GetResolutionKeyProperty(Type baseType)
    {
        if (baseType is null)
            throw new ArgumentNullException(nameof(baseType));

        var lookupType = baseType.IsGenericType ? baseType.GetGenericTypeDefinition() : baseType;

        return _hierarchy.Keys.FirstOrDefault(x => x.Type == lookupType)?.KeyProperty;
    }

    /// <summary>
    /// Gets a TypeId by its string identifier
    /// </summary>
    /// <param name="id">The string identifier to look up</param>
    /// <returns>The TypeId if found; otherwise, null</returns>
    public TypeId? GetTypeId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new InvalidEnumArgumentException("Id must not be default");

        return _ids.Keys.FirstOrDefault(x => x.Id == id);
    }

    /// <summary>
    /// Resolve type descendant by id
    /// </summary>
    /// <param name="id">The string ID to resolve</param>
    /// <returns>The resolved type or null if not found</returns>
    /// <exception cref="ArgumentNullException">Thrown when id is null or empty</exception>
    /// <exception cref="TypeResolutionException">Thrown when type resolution fails</exception>
    public Type? ResolveById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new InvalidEnumArgumentException("Id must not be default");

        return TypeId.TryParse(id, this)?.Type;
    }

    /// <summary>
    /// Resolve type descendant by key
    /// </summary>
    /// <param name="key">The resolution key to match</param>
    /// <param name="baseType">The base type to resolve from</param>
    /// <returns>The resolved type or null if not found</returns>
    /// <exception cref="ArgumentNullException">Thrown when key or baseType is null</exception>
    /// <exception cref="TypeResolutionException">Thrown when type resolution fails</exception>
    public Type? ResolveByKey(object key, Type baseType)
    {
        if (key is null)
            throw new ArgumentNullException(nameof(key));

        if (baseType is null)
            throw new ArgumentNullException(nameof(baseType));

        if (GetResolutionKeyProperty(baseType) is null)
            throw new TypeResolutionException(
                typeof(object),
                baseType,
                $"Type '{baseType}' has no {nameof(ResolutionKeyAttribute)}"
            );

        var descendants = GetImplementationDescendants(baseType).Where(x => x.HasKey && key.Equals(x.Key)).ToArray();
        if (descendants.Length > 1)
            throw new TypeResolutionException(
                typeof(object),
                baseType,
                $"Ambiguous resolution between {string.Join(", ", descendants.Select(x => x.Type.FullName))}"
            );

        return descendants.FirstOrDefault()?.Type;
    }

    /// <summary>
    /// Resolves type by signature
    /// </summary>
    /// <param name="signature">The signature to match against</param>
    /// <param name="baseType">The base type to resolve from</param>
    /// <param name="exact">Whether to require exact signature match</param>
    /// <returns>The resolved type or null if not found</returns>
    /// <exception cref="ArgumentNullException">Thrown when signature or baseType is null</exception>
    public Type? ResolveBySignature(IEnumerable<string> signature, Type baseType, bool exact = false)
    {
        if (signature is null)
            throw new ArgumentNullException(nameof(signature));

        if (baseType is null)
            throw new ArgumentNullException(nameof(baseType));

        var descendants = ResolveBySignature(TypeSignature.Create(signature), baseType, typeof(object));

        return (exact ? descendants.SingleOrDefault() : descendants.FirstOrDefault())?.Type;
    }

    /// <summary>
    /// Resolves descendant type of baseType by given source instance
    /// </summary>
    /// <param name="instance">The instance to resolve type from</param>
    /// <param name="baseType">The base type to resolve to</param>
    /// <returns>The resolved type or null if not found</returns>
    /// <exception cref="ArgumentNullException">Thrown when instance or baseType is null</exception>
    public Type? Resolve(object instance, Type baseType)
    {
        if (instance is null)
            throw new ArgumentNullException(nameof(instance));

        if (baseType is null)
            throw new ArgumentNullException(nameof(baseType));

        var resolutionIdProperty = GetResolutionIdProperty(baseType);
        if (resolutionIdProperty is not null)
        {
            // instance may not belong to hierarchy of baseType, so need to perform lookup for real property reference
            resolutionIdProperty = ResolveResolutionIdProperty(instance, resolutionIdProperty);

            var id = (string)resolutionIdProperty.GetValue(instance)!;

            return ResolveById(id);
        }

        var resolutionKeyProperty = GetResolutionKeyProperty(baseType);
        if (resolutionKeyProperty is not null)
        {
            // instance may not belong to hierarchy of baseType, so need to perform lookup for real property reference
            resolutionKeyProperty = ResolveResolutionKeyProperty(instance, resolutionKeyProperty);

            var key = resolutionKeyProperty.GetValue(instance)!;

            return ResolveByKey(key, baseType);
        }

        return ResolveBySignature(TypeSignature.Create(instance), baseType, instance.GetType()).FirstOrDefault()?.Type;
    }

    /// <summary>
    /// Internal resolve by TypeSignature implementation
    /// </summary>
    /// <param name="signature">The type signature to match</param>
    /// <param name="baseType">The base type to resolve from</param>
    /// <param name="assumedSourceType">The assumed source type for error reporting</param>
    /// <returns>Array of matching descendants ordered by match quality</returns>
    /// <exception cref="TypeResolutionException">Thrown when resolution is ambiguous</exception>
    private Descendant[] ResolveBySignature(TypeSignature signature, Type baseType, Type assumedSourceType)
    {
        var descendants = GetImplementationDescendants(baseType);

        var matches = descendants
            .Select(x => (descendant: x, match: x.Signature.GetMatchTo(signature)))
            .Where(x => x.match > 0)
            .OrderByDescending(x => x.match)
            .ToArray();

        if (matches.Length > 1)
        {
            var rivals = matches
                .TakeWhile(x => x.match == matches[0].match)
                .Select(x => x.descendant.Type.FullName)
                .ToArray();
            if (rivals.Length > 1)
                throw new TypeResolutionException(
                    assumedSourceType,
                    baseType,
                    $"Ambiguous resolution between {string.Join(", ", rivals)}"
                );
        }

        return matches.Select(x => x.descendant).ToArray();
    }

    /// <summary>
    /// Returns all direct implementations of base type.
    /// </summary>
    /// <param name="baseType">The base type to get descendants for</param>
    /// <returns>Array of descendant implementations</returns>
    private Descendant[] GetImplementationDescendants(Type baseType)
    {
        baseType = baseType.IsGenericType ? baseType.GetGenericTypeDefinition() : baseType;
        var node = _hierarchy.FirstOrDefault(x => x.Key.Type == baseType);

        return node.Key == null! ? Array.Empty<Descendant>() : node.Value.ToArray();
    }

    /// <summary>
    /// Resolves the actual resolution ID property for an instance
    /// </summary>
    /// <param name="instance">The instance to resolve property from</param>
    /// <param name="property">The expected property info</param>
    /// <returns>The actual PropertyInfo for the resolution ID</returns>
    private PropertyInfo ResolveResolutionIdProperty(object instance, PropertyInfo property)
    {
        var type = instance.GetType();

        // if instance type is hierarchy - no need to worry
        if (property.DeclaringType!.IsAssignableFrom(type))
            return property;

        var ancestor = new Ancestor(instance.GetType());
        if (!ancestor.HasIdProperty)
            throw new TypeResolutionException(
                type,
                property.DeclaringType,
                $"Source type '{type}' has no '{nameof(ResolutionIdAttribute)}'"
            );

        var realProperty = ancestor.IdProperty!;
        if (realProperty.PropertyType != typeof(string))
            throw new InvalidOperationException(
                $"Type '{ancestor.Type}' id property '{realProperty}' must be of type '{typeof(string)}'"
            );

        if (realProperty.Name != property.Name)
            throw new TypeResolutionException(
                type,
                property.DeclaringType,
                $"Source type '{type}' '{nameof(ResolutionIdAttribute)}' is assigned to property named '{realProperty.Name}'."
                    + $"Expected property name is '{property.Name}'."
            );

        return realProperty;
    }

    /// <summary>
    /// Resolves the actual resolution key property for an instance
    /// </summary>
    /// <param name="instance">The instance to resolve property from</param>
    /// <param name="property">The expected property info</param>
    /// <returns>The actual PropertyInfo for the resolution key</returns>
    private PropertyInfo ResolveResolutionKeyProperty(object instance, PropertyInfo property)
    {
        var type = instance.GetType();

        // if instance type is hierarchy - no need to worry
        if (property.DeclaringType!.IsAssignableFrom(type))
            return property;

        var ancestor = new Ancestor(instance.GetType());
        if (!ancestor.HasKeyProperty)
            throw new TypeResolutionException(
                type,
                property.DeclaringType,
                $"Source type '{type}' has no '{nameof(ResolutionKeyAttribute)}'"
            );

        var realProperty = ancestor.KeyProperty!;
        if (realProperty.Name != property.Name)
            throw new TypeResolutionException(
                type,
                property.DeclaringType,
                $"Source type '{type}' '{nameof(ResolutionKeyAttribute)}' is assigned to property named '{realProperty.Name}'."
                    + $"Expected property name is '{property.Name}'."
            );

        return realProperty;
    }
}
