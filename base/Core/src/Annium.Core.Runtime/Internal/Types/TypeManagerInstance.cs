using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Annium.Core.Runtime.Types;
using Annium.Logging;

namespace Annium.Core.Runtime.Internal.Types;

internal class TypeManagerInstance : ITypeManager, ILogSubject
{
    public ILogger Logger { get; }

    /// <summary>
    /// Contains collection of all types, collected for given Assembly
    /// </summary>
    public IReadOnlyCollection<Type> Types { get; }

    private readonly IReadOnlyDictionary<Ancestor, IReadOnlyCollection<Descendant>> _hierarchy;
    private readonly IReadOnlyDictionary<TypeId, Type> _ids;

    public TypeManagerInstance(
        Assembly assembly,
        ILogger logger
    )
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
    /// <param name="baseType"></param>
    public bool HasImplementations(Type baseType)
    {
        if (baseType is null)
            throw new ArgumentNullException(nameof(baseType));

        return GetImplementations(baseType).Length > 0;
    }

    /// <summary>
    /// Returns all direct implementations of <see cref="baseType"/>.
    /// </summary>
    /// <param name="baseType"></param>
    /// <returns></returns>
    public Type[] GetImplementations(Type baseType)
    {
        if (baseType is null)
            throw new ArgumentNullException(nameof(baseType));

        return GetImplementationDescendants(baseType).Select(x => x.Type).ToArray();
    }

    /// <summary>
    /// Returns resolution id property for given base type, if exists
    /// </summary>
    /// <param name="baseType"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public PropertyInfo? GetResolutionIdProperty(Type baseType)
    {
        if (baseType is null)
            throw new ArgumentNullException(nameof(baseType));

        var lookupType = baseType.IsGenericType ? baseType.GetGenericTypeDefinition() : baseType;

        var property = _hierarchy.Keys.FirstOrDefault(x => x.Type == lookupType)?.IdProperty;
        if (property is not null && property.PropertyType != typeof(string))
            throw new InvalidOperationException($"Type '{baseType}' id property '{property}' must be of type '{typeof(string)}'");

        return property;
    }

    /// <summary>
    /// Returns resolution key property for given base type, if exists
    /// </summary>
    /// <param name="baseType"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public PropertyInfo? GetResolutionKeyProperty(Type baseType)
    {
        if (baseType is null)
            throw new ArgumentNullException(nameof(baseType));

        var lookupType = baseType.IsGenericType ? baseType.GetGenericTypeDefinition() : baseType;

        return _hierarchy.Keys.FirstOrDefault(x => x.Type == lookupType)?.KeyProperty;
    }

    public TypeId? GetTypeId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new InvalidEnumArgumentException("Id must not be default");

        return _ids.Keys.FirstOrDefault(x => x.Id == id);
    }

    /// <summary>
    /// Resolve type descendant by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="TypeResolutionException"></exception>
    public Type? ResolveById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new InvalidEnumArgumentException("Id must not be default");

        return TypeId.TryParse(id, this)?.Type;
    }

    /// <summary>
    /// Resolve type descendant by key
    /// </summary>
    /// <param name="key"></param>
    /// <param name="baseType"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="TypeResolutionException"></exception>
    public Type? ResolveByKey(object key, Type baseType)
    {
        if (key is null)
            throw new ArgumentNullException(nameof(key));

        if (baseType is null)
            throw new ArgumentNullException(nameof(baseType));

        if (GetResolutionKeyProperty(baseType) is null)
            throw new TypeResolutionException(typeof(object), baseType, $"Type '{baseType}' has no {nameof(ResolutionKeyAttribute)}");

        var descendants = GetImplementationDescendants(baseType).Where(x => x.HasKey && key.Equals(x.Key)).ToArray();
        if (descendants.Length > 1)
            throw new TypeResolutionException(typeof(object), baseType,
                $"Ambiguous resolution between {string.Join(", ", descendants.Select(x => x.Type.FullName))}");

        return descendants.FirstOrDefault()?.Type;
    }

    /// <summary>
    /// Resolves type by signature
    /// </summary>
    /// <param name="signature"></param>
    /// <param name="baseType"></param>
    /// <param name="exact"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
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
    /// <param name="instance"></param>
    /// <param name="baseType"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
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
    /// <param name="signature"></param>
    /// <param name="baseType"></param>
    /// <param name="assumedSourceType"></param>
    /// <returns></returns>
    /// <exception cref="TypeResolutionException"></exception>
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
            var rivals = matches.TakeWhile(x => x.match == matches[0].match).Select(x => x.descendant.Type.FullName).ToArray();
            if (rivals.Length > 1)
                throw new TypeResolutionException(assumedSourceType, baseType, $"Ambiguous resolution between {string.Join(", ", rivals)}");
        }

        return matches.Select(x => x.descendant).ToArray();
    }


    /// <summary>
    /// Returns all direct implementations of <see cref="baseType"/>.
    /// </summary>
    /// <param name="baseType"></param>
    /// <returns></returns>
    private Descendant[] GetImplementationDescendants(Type baseType)
    {
        baseType = baseType.IsGenericType ? baseType.GetGenericTypeDefinition() : baseType;
        var node = _hierarchy.FirstOrDefault(x => x.Key.Type == baseType);

        return node.Key == null! ? Array.Empty<Descendant>() : node.Value.ToArray();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="property"></param>
    /// <returns></returns>
    private PropertyInfo ResolveResolutionIdProperty(object instance, PropertyInfo property)
    {
        var type = instance.GetType();

        // if instance type is hierarchy - no need to worry
        if (property.DeclaringType!.IsAssignableFrom(type))
            return property;

        var ancestor = new Ancestor(instance.GetType());
        if (!ancestor.HasIdProperty)
            throw new TypeResolutionException(type, property.DeclaringType, $"Source type '{type}' has no '{nameof(ResolutionIdAttribute)}'");

        var realProperty = ancestor.IdProperty!;
        if (realProperty.PropertyType != typeof(string))
            throw new InvalidOperationException($"Type '{ancestor.Type}' id property '{realProperty}' must be of type '{typeof(string)}'");

        if (realProperty.Name != property.Name)
            throw new TypeResolutionException(
                type, property.DeclaringType,
                $"Source type '{type}' '{nameof(ResolutionIdAttribute)}' is assigned to property named '{realProperty.Name}'." +
                $"Expected property name is '{property.Name}'."
            );

        return realProperty;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="property"></param>
    /// <returns></returns>
    private PropertyInfo ResolveResolutionKeyProperty(object instance, PropertyInfo property)
    {
        var type = instance.GetType();

        // if instance type is hierarchy - no need to worry
        if (property.DeclaringType!.IsAssignableFrom(type))
            return property;

        var ancestor = new Ancestor(instance.GetType());
        if (!ancestor.HasKeyProperty)
            throw new TypeResolutionException(type, property.DeclaringType, $"Source type '{type}' has no '{nameof(ResolutionKeyAttribute)}'");

        var realProperty = ancestor.KeyProperty!;
        if (realProperty.Name != property.Name)
            throw new TypeResolutionException(
                type, property.DeclaringType,
                $"Source type '{type}' '{nameof(ResolutionKeyAttribute)}' is assigned to property named '{realProperty.Name}'." +
                $"Expected property name is '{property.Name}'."
            );

        return realProperty;
    }
}