using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Annium.Net.Types.Models;

namespace Annium.Net.Types.Extensions;

/// <summary>
/// Extension methods for namespace manipulation and validation operations.
/// </summary>
public static class NamespaceExtensions
{
    /// <summary>
    /// Determines whether this namespace starts with the specified target namespace.
    /// </summary>
    /// <param name="ns">The namespace to check</param>
    /// <param name="target">The target namespace prefix</param>
    /// <returns>True if this namespace starts with the target, false otherwise</returns>
    public static bool StartsWith(this Namespace ns, Namespace target)
    {
        if (target.Count > ns.Count)
            return false;

        for (var i = 0; i < target.Count; i++)
            if (ns[i] != target[i])
                return false;

        return true;
    }

    /// <summary>
    /// Creates a new namespace by removing the target namespace prefix from this namespace.
    /// </summary>
    /// <param name="ns">The source namespace</param>
    /// <param name="target">The namespace prefix to remove</param>
    /// <returns>A new namespace with the prefix removed</returns>
    /// <exception cref="ArgumentException">Thrown when the namespace doesn't start with the target</exception>
    public static Namespace From(this Namespace ns, Namespace target)
    {
        if (!ns.StartsWith(target))
            throw new ArgumentException($"Namespace {ns} doesn't contain namespace {ns}");

        return Namespace.New(ns.Skip(target.Count).ToArray());
    }

    /// <summary>
    /// Creates a new namespace by prepending the target namespace to this namespace.
    /// </summary>
    /// <param name="ns">The namespace to prepend to</param>
    /// <param name="target">The namespace to prepend</param>
    /// <returns>A new namespace with the target prepended</returns>
    public static Namespace Prepend(this Namespace ns, Namespace target)
    {
        return Namespace.New(target.Concat(ns).ToArray());
    }

    /// <summary>
    /// Creates a new namespace by appending the target namespace to this namespace.
    /// </summary>
    /// <param name="ns">The namespace to append to</param>
    /// <param name="target">The namespace to append</param>
    /// <returns>A new namespace with the target appended</returns>
    public static Namespace Append(this Namespace ns, Namespace target)
    {
        return Namespace.New(ns.Concat(target).ToArray());
    }

    /// <summary>
    /// Converts this namespace to a file system path by combining with a base path.
    /// </summary>
    /// <param name="ns">The namespace to convert</param>
    /// <param name="basePath">The base directory path</param>
    /// <returns>A file system path representing the namespace structure</returns>
    public static string ToPath(this Namespace ns, string basePath) =>
        Path.Combine(basePath, Path.Combine(ns.ToArray()));

    /// <summary>
    /// Validates that the namespace parts are not null or empty.
    /// </summary>
    /// <typeparam name="T">The type of namespace collection</typeparam>
    /// <param name="ns">The namespace parts to validate</param>
    /// <returns>The validated namespace parts</returns>
    /// <exception cref="ArgumentException">Thrown when any namespace part is null or empty</exception>
    internal static T EnsureValidNamespace<T>(this T ns)
        where T : IEnumerable<string>
    {
        if (ns.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException($"Namespace {ns.ToNamespaceString()} contains empty parts");

        return ns;
    }
}
