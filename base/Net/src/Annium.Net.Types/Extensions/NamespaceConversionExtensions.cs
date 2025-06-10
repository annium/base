using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Net.Types.Models;

namespace Annium.Net.Types.Extensions;

/// <summary>
/// Extension methods for converting between different namespace representations.
/// </summary>
public static class NamespaceConversionExtensions
{
    /// <summary>
    /// Extracts the namespace from a .NET type and converts it to a Namespace instance.
    /// </summary>
    /// <param name="type">The type to extract namespace from</param>
    /// <returns>The namespace of the type, or empty namespace if type has no namespace</returns>
    public static Namespace GetNamespace(this Type type) => (type.Namespace ?? string.Empty).ToNamespace();

    /// <summary>
    /// Converts a dot-separated namespace string to a Namespace instance.
    /// </summary>
    /// <param name="ns">The namespace string to convert</param>
    /// <returns>A new Namespace instance representing the string</returns>
    public static Namespace ToNamespace(this string ns) => Namespace.New(ns.ToNamespaceArray());

    /// <summary>
    /// Converts an enumerable of namespace parts to a Namespace instance.
    /// </summary>
    /// <param name="ns">The namespace parts to convert</param>
    /// <returns>A new Namespace instance representing the parts</returns>
    public static Namespace ToNamespace(this IEnumerable<string> ns) => Namespace.New(ns);

    /// <summary>
    /// Converts an enumerable of namespace parts to a dot-separated string.
    /// </summary>
    /// <param name="ns">The namespace parts to convert</param>
    /// <returns>A dot-separated namespace string</returns>
    public static string ToNamespaceString(this IEnumerable<string> ns) => string.Join('.', ns);

    /// <summary>
    /// Converts a namespace string to an array of namespace parts with validation.
    /// </summary>
    /// <param name="ns">The namespace string to split</param>
    /// <returns>An array of validated namespace parts</returns>
    private static string[] ToNamespaceArray(this string ns) =>
        ns switch
        {
            "" => Array.Empty<string>(),
            _ => ns.Split('.').ToArray().EnsureValidNamespace(),
        };
}
