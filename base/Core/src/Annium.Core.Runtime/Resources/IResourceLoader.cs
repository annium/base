using System.Collections.Generic;
using System.Reflection;

namespace Annium.Core.Runtime.Resources;

/// <summary>
/// Interface for loading embedded resources from assemblies
/// </summary>
public interface IResourceLoader
{
    /// <summary>
    /// Loads all embedded resources with the specified prefix from the calling assembly
    /// </summary>
    /// <param name="prefix">The prefix to filter resources by</param>
    /// <returns>Collection of loaded resources</returns>
    IReadOnlyCollection<IResource> Load(string prefix);

    /// <summary>
    /// Loads all embedded resources with the specified prefix from the given assembly
    /// </summary>
    /// <param name="prefix">The prefix to filter resources by</param>
    /// <param name="assembly">The assembly to load resources from</param>
    /// <returns>Collection of loaded resources</returns>
    IReadOnlyCollection<IResource> Load(string prefix, Assembly assembly);
}
