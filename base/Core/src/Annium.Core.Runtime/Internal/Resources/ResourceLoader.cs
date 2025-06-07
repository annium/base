using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Annium.Core.Runtime.Resources;
using Annium.Reflection;

namespace Annium.Core.Runtime.Internal.Resources;

/// <summary>
/// Internal implementation of resource loader that loads embedded resources from assemblies
/// </summary>
internal class ResourceLoader : IResourceLoader
{
    /// <summary>
    /// Loads all embedded resources with the specified prefix from the calling assembly
    /// </summary>
    /// <param name="prefix">The prefix to filter resources by</param>
    /// <returns>Collection of loaded resources</returns>
    public IReadOnlyCollection<IResource> Load(string prefix) => Load(prefix, Assembly.GetCallingAssembly());

    /// <summary>
    /// Loads all embedded resources with the specified prefix from the given assembly
    /// </summary>
    /// <param name="prefix">The prefix to filter resources by</param>
    /// <param name="assembly">The assembly to load resources from</param>
    /// <returns>Collection of loaded resources</returns>
    public IReadOnlyCollection<IResource> Load(string prefix, Assembly assembly)
    {
        prefix = $"{assembly.ShortName()}.{prefix}";

        var names = assembly.GetManifestResourceNames();

        return names
            .Where(r => r.StartsWith(prefix))
            .Select(r =>
            {
                var name = r[prefix.Length..];
                using var rs = assembly.GetManifestResourceStream(r)!;
                rs.Seek(0, SeekOrigin.Begin);
                using var ms = new MemoryStream();
                rs.CopyTo(ms);

                return new Resource(name, ms.ToArray().AsMemory());
            })
            .ToArray();
    }
}
