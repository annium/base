using System;
using Annium.Core.Runtime.Types;

namespace Annium.Net.Types.Refs;

/// <summary>
/// Type reference for interface types with support for generic arguments.
/// Implements IGenericModelRef to provide namespace, name, and generic argument information.
/// </summary>
/// <param name="Namespace">The namespace containing the interface</param>
/// <param name="Name">The name of the interface</param>
/// <param name="Args">The generic type arguments for the interface</param>
[ResolutionKeyValue(RefType.Interface)]
public sealed record InterfaceRef(string Namespace, string Name, params IRef[] Args) : IGenericModelRef
{
    /// <summary>
    /// Gets the type of reference this represents.
    /// </summary>
    public RefType Type => RefType.Interface;

    /// <summary>
    /// Returns the hash code for this interface reference.
    /// </summary>
    /// <returns>A hash code based on namespace, name, and generic arguments</returns>
    public override int GetHashCode() => HashCode.Combine(Namespace, Name, HashCodeSeq.Combine(Args));

    /// <summary>
    /// Determines whether the specified interface reference is equal to this interface reference.
    /// </summary>
    /// <param name="other">The interface reference to compare with this reference</param>
    /// <returns>True if the references are equal, false otherwise</returns>
    public bool Equals(InterfaceRef? other) => GetHashCode() == other?.GetHashCode();
}
