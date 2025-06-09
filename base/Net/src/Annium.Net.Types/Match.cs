using System;
using System.Reflection;
using Annium.Reflection;

namespace Annium.Net.Types;

/// <summary>
/// Utility class providing predicate functions for matching types based on various criteria.
/// Used primarily with mapper configuration methods for type filtering and selection.
/// </summary>
public static class Match
{
    /// <summary>
    /// Creates a predicate that matches types exactly equal to the target type.
    /// </summary>
    /// <param name="target">The type to match against</param>
    /// <returns>A predicate function that returns true for types equal to the target</returns>
    public static Predicate<Type> Is(Type target) => type => type == target;

    /// <summary>
    /// Creates a predicate that matches types derived from the target type.
    /// </summary>
    /// <param name="target">The base type to check inheritance from</param>
    /// <param name="self">Whether to include the target type itself in the match</param>
    /// <returns>A predicate function that returns true for types derived from the target</returns>
    public static Predicate<Type> IsDerivedFrom(Type target, bool self = false) =>
        type => type.IsDerivedFrom(target, self);

    /// <summary>
    /// Creates a predicate that matches types in the specified namespace.
    /// </summary>
    /// <param name="ns">The namespace to match</param>
    /// <returns>A predicate function that returns true for types in the specified namespace</returns>
    public static Predicate<Type> NamespaceIs(string ns) => type => type.Namespace == ns;

    /// <summary>
    /// Creates a predicate that matches types whose namespace starts with the specified prefix.
    /// </summary>
    /// <param name="ns">The namespace prefix to match</param>
    /// <returns>A predicate function that returns true for types whose namespace starts with the prefix</returns>
    public static Predicate<Type> NamespaceStartsWith(string ns) => type => type.Namespace?.StartsWith(ns) ?? false;

    /// <summary>
    /// Creates a predicate that matches types from the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly to match types from</param>
    /// <returns>A predicate function that returns true for types from the specified assembly</returns>
    public static Predicate<Type> IsFromAssembly(Assembly assembly) => type => type.Assembly == assembly;
}
