using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Annium.Net.Types.Extensions;

namespace Annium.Net.Types.Models;

/// <summary>
/// Represents a namespace as an immutable collection of string parts.
/// Provides validation, conversion, and manipulation methods for namespaces.
/// Implements IReadOnlyList&lt;string&gt; for easy enumeration and indexing.
/// </summary>
public sealed record Namespace : IReadOnlyList<string>
{
    #region static

    /// <summary>
    /// Creates a new namespace from an enumerable of string parts with validation.
    /// </summary>
    /// <param name="ns">The namespace parts to create the namespace from</param>
    /// <returns>A new validated namespace instance</returns>
    public static Namespace New(IEnumerable<string> ns) => new(ns.ToArray().EnsureValidNamespace());

    /// <summary>
    /// Creates a new namespace from a read-only list of string parts with validation.
    /// </summary>
    /// <param name="ns">The namespace parts to create the namespace from</param>
    /// <returns>A new validated namespace instance</returns>
    public static Namespace New(IReadOnlyList<string> ns) => new(ns.EnsureValidNamespace());

    #endregion

    #region instance

    /// <summary>
    /// Gets the number of parts in this namespace.
    /// </summary>
    public int Count => _parts.Count;

    /// <summary>
    /// Gets the namespace part at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the namespace part to get</param>
    /// <returns>The namespace part at the specified index</returns>
    public string this[int index] => _parts[index];

    /// <summary>
    /// The internal storage for namespace parts.
    /// </summary>
    private readonly IReadOnlyList<string> _parts;

    /// <summary>
    /// Initializes a new namespace with the specified parts.
    /// </summary>
    /// <param name="parts">The validated namespace parts</param>
    private Namespace(IReadOnlyList<string> parts)
    {
        _parts = parts;
    }

    /// <summary>
    /// Returns a non-generic enumerator that iterates through the namespace parts.
    /// </summary>
    /// <returns>A non-generic enumerator for the namespace parts</returns>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Returns an enumerator that iterates through the namespace parts.
    /// </summary>
    /// <returns>An enumerator for the namespace parts</returns>
    public IEnumerator<string> GetEnumerator() => _parts.GetEnumerator();

    /// <summary>
    /// Returns the string representation of this namespace using dot notation.
    /// </summary>
    /// <returns>The namespace as a dot-separated string</returns>
    public override string ToString() => _parts.ToNamespaceString();

    /// <summary>
    /// Determines whether the specified namespace is equal to this namespace.
    /// </summary>
    /// <param name="other">The namespace to compare with this namespace</param>
    /// <returns>True if the namespaces are equal, false otherwise</returns>
    public bool Equals(Namespace? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return _parts.SequenceEqual(other._parts);
    }

    /// <summary>
    /// Returns the hash code for this namespace.
    /// </summary>
    /// <returns>A hash code based on the namespace parts</returns>
    public override int GetHashCode() => HashCodeSeq.Combine(_parts);

    #endregion

    #region operators

    /// <summary>
    /// Implicitly converts a string to a namespace.
    /// </summary>
    /// <param name="value">The string to convert</param>
    /// <returns>A namespace created from the string</returns>
    public static implicit operator Namespace(string value) => value.ToNamespace();

    /// <summary>
    /// Implicitly converts a namespace to its string representation.
    /// </summary>
    /// <param name="value">The namespace to convert</param>
    /// <returns>The string representation of the namespace</returns>
    public static implicit operator string(Namespace value) => value.ToString();

    #endregion
}
