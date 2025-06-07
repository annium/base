using System;
using System.Linq;
using Annium.Core.Runtime.Internal.Types;

namespace Annium.Core.Runtime.Types;

/// <summary>
/// Abstract base record for representing type identities in the runtime system
/// </summary>
public abstract record TypeId
{
    /// <summary>
    /// Creates a TypeId for the specified type
    /// </summary>
    /// <param name="type">The type to create an ID for</param>
    /// <returns>A TypeId representing the specified type</returns>
    public static TypeId Create(Type type)
    {
        if (type.IsGenericParameter)
            throw new InvalidOperationException("TypeId is not valid for generic parameters");

        if (!type.IsGenericType)
            return new NonGenericTypeId(type);

        if (type.IsGenericTypeDefinition)
            return new OpenGenericTypeId(type);

        return new ClosedGenericTypeId(type);
    }

    /// <summary>
    /// Attempts to parse a string ID into a TypeId using the specified type manager
    /// </summary>
    /// <param name="id">The string ID to parse</param>
    /// <param name="tm">The type manager to use for resolution</param>
    /// <returns>The parsed TypeId or null if parsing fails</returns>
    public static TypeId? TryParse(string id, ITypeManager tm)
    {
        // if not constructed generic type - try resolve from TypeManager
        if (!id.Contains('<') && !id.Contains('>'))
            return tm.GetTypeId(id);

        var laIndex = id.IndexOf('<');
        var raIndex = id.LastIndexOf('>');
        if (laIndex < 0 || raIndex < 0 || raIndex < laIndex)
            throw new ArgumentException($"String id '{id}' has invalid format");

        var name = id[..laIndex];
        var baseId = tm.GetTypeId(name);
        if (baseId is null)
            return null;

        var rawArgs = id.Substring(laIndex + 1, raIndex - laIndex - 1).Split(',');
        var args = rawArgs.Select(x => TryParse(x, tm)).ToArray();
        if (args.Any(x => x is null))
            return null;

        var type = baseId.Type.MakeGenericType(args.Select(x => x!.Type).ToArray());

        return Create(type);
    }

    /// <summary>
    /// Gets the formatted name for a type including namespace
    /// </summary>
    /// <param name="type">The type to get the name for</param>
    /// <returns>The formatted name string</returns>
    protected static string GetName(Type type)
    {
        var ns = type.Namespace;
        var name = type.Name;

        return $"{ns}:{name}";
    }

    /// <summary>
    /// The actual Type this ID represents
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// The base type (for generic types, this is the generic type definition)
    /// </summary>
    public Type BaseType { get; }

    /// <summary>
    /// The formatted name of the type
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The unique string identifier for this type
    /// </summary>
    public string Id { get; protected init; } = string.Empty;

    /// <summary>
    /// Initializes a new TypeId with the specified type and name
    /// </summary>
    /// <param name="type">The type this ID represents</param>
    /// <param name="name">The formatted name of the type</param>
    protected TypeId(Type type, string name)
        : this(type, type, name) { }

    /// <summary>
    /// Initializes a new TypeId with the specified type, base type, and name
    /// </summary>
    /// <param name="type">The type this ID represents</param>
    /// <param name="baseType">The base type</param>
    /// <param name="name">The formatted name of the type</param>
    protected TypeId(Type type, Type baseType, string name)
    {
        Type = type;
        BaseType = baseType;
        Name = name;
    }

    /// <summary>
    /// Determines whether this TypeId equals another TypeId based on their string IDs
    /// </summary>
    /// <param name="other">The other TypeId to compare with</param>
    /// <returns>True if the IDs are equal; otherwise, false</returns>
    public virtual bool Equals(TypeId? other) => Id == other?.Id;

    /// <summary>
    /// Gets the hash code for this TypeId based on its string ID
    /// </summary>
    /// <returns>The hash code of the string ID</returns>
    public override int GetHashCode() => Id.GetHashCode();
}
