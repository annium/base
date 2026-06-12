using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.Runtime.Types;

namespace Annium.Net.Types.Refs;

/// <summary>
/// Type reference for struct and class types with support for generic arguments.
/// Implements IGenericModelRef to provide namespace, name, and generic argument information.
/// </summary>
/// <param name="Namespace">The namespace containing the struct</param>
/// <param name="Name">The name of the struct</param>
/// <param name="Args">The generic type arguments for the struct</param>
[ResolutionKeyValue(RefType.Struct)]
public sealed record StructRef(string Namespace, string Name, params IReadOnlyList<IRef> Args) : IGenericModelRef
{
    /// <summary>
    /// Gets the type of reference this represents.
    /// </summary>
    public RefType Type => RefType.Struct;

    /// <summary>
    /// Returns the hash code for this struct reference.
    /// </summary>
    /// <returns>A hash code based on namespace, name, and generic arguments</returns>
    public override int GetHashCode()
    {
        var args = new HashCode();
        foreach (var arg in Args)
            args.Add(arg); // HashCode.Add is null-tolerant (null hashes to 0), unlike HashCodeSeq.Combine
        return HashCode.Combine(Namespace, Name, args.ToHashCode());
    }

    /// <summary>
    /// Determines whether the specified struct reference is equal to this struct reference.
    /// </summary>
    /// <param name="other">The struct reference to compare with this reference</param>
    /// <returns>True if the references are equal, false otherwise</returns>
    public bool Equals(StructRef? other) =>
        other is not null && Namespace == other.Namespace && Name == other.Name && Args.SequenceEqual(other.Args);
}
