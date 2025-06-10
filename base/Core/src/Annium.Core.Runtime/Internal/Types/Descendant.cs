using System;
using System.Reflection;
using Annium.Core.Runtime.Types;

namespace Annium.Core.Runtime.Internal.Types;

/// <summary>
/// Internal representation of a descendant type in the type hierarchy
/// </summary>
internal sealed class Descendant
{
    /// <summary>
    /// The descendant type
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// The type signature used for matching
    /// </summary>
    public TypeSignature Signature { get; }

    /// <summary>
    /// The type ID for this descendant
    /// </summary>
    public TypeId Id { get; }

    /// <summary>
    /// The resolution key value if specified
    /// </summary>
    public object? Key { get; }

    /// <summary>
    /// Whether this descendant has a resolution key
    /// </summary>
    public bool HasKey { get; }

    /// <summary>
    /// Initializes a new instance of Descendant for the specified type
    /// </summary>
    /// <param name="type">The type to create a descendant representation for</param>
    public Descendant(Type type)
    {
        Type = type;
        Signature = TypeSignature.Create(type);
        Id = type.GetTypeId();
        var keyAttribute = type.GetTypeInfo().GetCustomAttribute<ResolutionKeyValueAttribute>();
        Key = keyAttribute?.Key;
        HasKey = keyAttribute != null;
    }

    /// <summary>
    /// Returns the string representation of this descendant
    /// </summary>
    /// <returns>The name of the descendant type</returns>
    public override string ToString() => Type.Name;
}
