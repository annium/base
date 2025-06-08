using System;
using System.Collections.Generic;
using System.Linq;

namespace Annium.Core.Runtime.Internal.Types;

/// <summary>
/// Internal representation of a type signature for type matching and resolution
/// </summary>
internal sealed class TypeSignature : IEquatable<TypeSignature>
{
    /// <summary>
    /// Creates a TypeSignature from an object instance
    /// </summary>
    /// <param name="instance">The object instance to create signature from</param>
    /// <returns>A TypeSignature based on the instance's type</returns>
    public static TypeSignature Create(object instance) =>
        Create(instance.GetType() ?? throw new ArgumentNullException(nameof(instance)));

    /// <summary>
    /// Creates a TypeSignature from a Type
    /// </summary>
    /// <param name="type">The type to create signature from</param>
    /// <returns>A TypeSignature based on the type's properties</returns>
    public static TypeSignature Create(Type type) => Create(type.GetProperties().Select(x => x.Name));

    /// <summary>
    /// Creates a TypeSignature from an array of property names
    /// </summary>
    /// <param name="signature">The property names that form the signature</param>
    /// <returns>A TypeSignature based on the property names</returns>
    public static TypeSignature Create(params string[] signature) => Create(signature.AsEnumerable());

    /// <summary>
    /// Creates a TypeSignature from a collection of property names
    /// </summary>
    /// <param name="signature">The property names that form the signature</param>
    /// <returns>A TypeSignature based on the property names</returns>
    public static TypeSignature Create(IEnumerable<string> signature)
    {
        var normalizedSignature = signature.Select(x => x.ToLowerInvariant()).Distinct().OrderBy(x => x).ToArray();

        return new TypeSignature(normalizedSignature);
    }

    /// <summary>
    /// The number of properties in this signature
    /// </summary>
    public int Size => _signature.Count;

    /// <summary>
    /// The normalized property names that form this signature
    /// </summary>
    private readonly IReadOnlyCollection<string> _signature;

    /// <summary>
    /// Initializes a new instance of TypeSignature with the specified property names
    /// </summary>
    /// <param name="signature">The collection of property names</param>
    private TypeSignature(IReadOnlyCollection<string> signature)
    {
        if (signature is null)
            throw new ArgumentNullException(nameof(signature));

        _signature = signature;
    }

    /// <summary>
    /// Calculates the match score between this signature and a target signature
    /// </summary>
    /// <param name="target">The target signature to match against</param>
    /// <returns>A score indicating quality of match (higher is better)</returns>
    public int GetMatchTo(TypeSignature target)
    {
        // get number of target signature's item, this signature contains
        var matches = target._signature.Count(_signature.Contains);

        return matches * 100 - Size;

        // // if target signature contains any different items - no match
        // if (target._signature.Any(x => !_signature.Contains(x)))
        //     return 0;
        //
        // // get number of target signature's item, this signature contains
        // double matches = target._signature.Count(_signature.Contains);
        //
        // // return percentage relative to signature size - second layer of matching
        // return (int) Math.Floor(matches / Size);
    }

    /// <summary>
    /// Determines whether this TypeSignature equals another TypeSignature
    /// </summary>
    /// <param name="obj">The other TypeSignature to compare with</param>
    /// <returns>True if the signatures are equal; otherwise, false</returns>
    public bool Equals(TypeSignature? obj) => GetHashCode() == obj?.GetHashCode();

    /// <summary>
    /// Determines whether this TypeSignature equals another object
    /// </summary>
    /// <param name="obj">The object to compare with</param>
    /// <returns>True if the object is a TypeSignature and equal to this one; otherwise, false</returns>
    public override bool Equals(object? obj) => obj is TypeSignature sign && Equals(sign);

    /// <summary>
    /// Gets the hash code for this TypeSignature
    /// </summary>
    /// <returns>The hash code based on the signature properties</returns>
    public override int GetHashCode() => HashCode.Combine(_signature);

    /// <summary>
    /// Returns the string representation of this TypeSignature
    /// </summary>
    /// <returns>A comma-separated string of property names</returns>
    public override string ToString() => string.Join(", ", _signature);

    /// <summary>
    /// Determines whether two TypeSignature instances are equal
    /// </summary>
    /// <param name="a">The first TypeSignature to compare</param>
    /// <param name="b">The second TypeSignature to compare</param>
    /// <returns>True if the TypeSignatures are equal; otherwise, false</returns>
    public static bool operator ==(TypeSignature? a, TypeSignature? b)
    {
        // If both are null, or both are same instance, return true.
        if (ReferenceEquals(a, b))
            return true;

        // If one is null, but not both, return false.
        if (a is null || b is null)
            return false;

        // Return true if the fields match:
        return a.Equals(b);
    }

    /// <summary>
    /// Determines whether two TypeSignature instances are not equal
    /// </summary>
    /// <param name="a">The first TypeSignature to compare</param>
    /// <param name="b">The second TypeSignature to compare</param>
    /// <returns>True if the TypeSignatures are not equal; otherwise, false</returns>
    public static bool operator !=(TypeSignature a, TypeSignature b) => !(a == b);
}
