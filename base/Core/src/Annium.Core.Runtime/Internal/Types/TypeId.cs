using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.Runtime.Types;

namespace Annium.Core.Runtime.Internal.Types;

/// <summary>
/// Internal implementation of TypeId for non-generic types
/// </summary>
internal sealed record NonGenericTypeId : TypeId
{
    /// <summary>
    /// Initializes a new instance of NonGenericTypeId for the specified type
    /// </summary>
    /// <param name="type">The non-generic type to create an ID for</param>
    public NonGenericTypeId(Type type)
        : base(type, GetName(type))
    {
        Id = Name;
    }

    /// <summary>
    /// Determines whether this NonGenericTypeId equals another NonGenericTypeId
    /// </summary>
    /// <param name="other">The other NonGenericTypeId to compare with</param>
    /// <returns>True if the IDs are equal; otherwise, false</returns>
    public bool Equals(NonGenericTypeId? other) => Id == other?.Id;

    /// <summary>
    /// Gets the hash code for this NonGenericTypeId
    /// </summary>
    /// <returns>The hash code of the string ID</returns>
    public override int GetHashCode() => Id.GetHashCode();
}

/// <summary>
/// Internal implementation of TypeId for open generic types (generic type definitions)
/// </summary>
internal sealed record OpenGenericTypeId : TypeId
{
    /// <summary>
    /// Initializes a new instance of OpenGenericTypeId for the specified generic type definition
    /// </summary>
    /// <param name="type">The open generic type to create an ID for</param>
    public OpenGenericTypeId(Type type)
        : base(type, type, GetName(type))
    {
        Id = GetName(type);
    }

    /// <summary>
    /// Determines whether this OpenGenericTypeId equals another OpenGenericTypeId
    /// </summary>
    /// <param name="other">The other OpenGenericTypeId to compare with</param>
    /// <returns>True if the IDs are equal; otherwise, false</returns>
    public bool Equals(OpenGenericTypeId? other) => Id == other?.Id;

    /// <summary>
    /// Gets the hash code for this OpenGenericTypeId
    /// </summary>
    /// <returns>The hash code of the string ID</returns>
    public override int GetHashCode() => Id.GetHashCode();
}

/// <summary>
/// Internal implementation of TypeId for closed generic types (constructed generic types)
/// </summary>
internal sealed record ClosedGenericTypeId : TypeId
{
    /// <summary>
    /// Creates the string ID for a closed generic type with its type arguments
    /// </summary>
    /// <param name="type">The closed generic type</param>
    /// <param name="args">The type arguments</param>
    /// <returns>The formatted string ID</returns>
    private static string GetId(Type type, IReadOnlyCollection<TypeId> args) =>
        $"{GetName(type)}<{string.Join(',', args.Select(x => x.Id))}>";

    /// <summary>
    /// The type arguments for this closed generic type
    /// </summary>
    public IReadOnlyCollection<TypeId> Arguments { get; }

    /// <summary>
    /// Initializes a new instance of ClosedGenericTypeId for the specified constructed generic type
    /// </summary>
    /// <param name="type">The closed generic type to create an ID for</param>
    public ClosedGenericTypeId(Type type)
        : base(type, type.GetGenericTypeDefinition(), GetName(type))
    {
        Arguments = type.GetGenericArguments().Select(Create).ToArray();
        Id = GetId(type, Arguments);
    }

    /// <summary>
    /// Determines whether this ClosedGenericTypeId equals another ClosedGenericTypeId
    /// </summary>
    /// <param name="other">The other ClosedGenericTypeId to compare with</param>
    /// <returns>True if the IDs are equal; otherwise, false</returns>
    public bool Equals(ClosedGenericTypeId? other) => Id == other?.Id;

    /// <summary>
    /// Gets the hash code for this ClosedGenericTypeId
    /// </summary>
    /// <returns>The hash code of the string ID</returns>
    public override int GetHashCode() => Id.GetHashCode();
}
