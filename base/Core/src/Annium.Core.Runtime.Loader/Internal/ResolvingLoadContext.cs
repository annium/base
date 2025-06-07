using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;

namespace Annium.Core.Runtime.Loader.Internal;

/// <summary>
/// Custom assembly load context that uses configurable resolvers to load assemblies
/// </summary>
internal class ResolvingLoadContext : AssemblyLoadContext
{
    /// <summary>
    /// Collection of path-based assembly resolvers
    /// </summary>
    private readonly IReadOnlyCollection<Func<AssemblyName, string?>> _pathResolvers;

    /// <summary>
    /// Collection of byte array-based assembly resolvers
    /// </summary>
    private readonly IReadOnlyCollection<Func<AssemblyName, Task<byte[]>?>> _byteArrayResolvers;

    /// <summary>
    /// Initializes a new instance of ResolvingLoadContext with specified resolvers
    /// </summary>
    /// <param name="pathResolvers">Collection of functions that resolve assembly names to file paths</param>
    /// <param name="byteArrayResolvers">Collection of functions that resolve assembly names to byte arrays</param>
    public ResolvingLoadContext(
        IReadOnlyCollection<Func<AssemblyName, string?>> pathResolvers,
        IReadOnlyCollection<Func<AssemblyName, Task<byte[]>?>> byteArrayResolvers
    )
        : base(true)
    {
        _pathResolvers = pathResolvers;
        _byteArrayResolvers = byteArrayResolvers;
    }

    /// <summary>
    /// Attempts to load an assembly using configured resolvers
    /// </summary>
    /// <param name="assemblyName">The name of the assembly to load</param>
    /// <returns>The loaded assembly or null if not found</returns>
    protected override Assembly? Load(AssemblyName assemblyName)
    {
        // try resolve by path
        var path = _pathResolvers.Select(x => x(assemblyName)).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
        if (!string.IsNullOrWhiteSpace(path))
            return LoadFromAssemblyPath(path);

        // try resolve from stream by fetching byteArray
        var byteArray = _byteArrayResolvers.Select(x => x(assemblyName)?.Result).FirstOrDefault(x => x != null);
        if (byteArray != null)
        {
            using var ms = new MemoryStream(byteArray);
            return LoadFromStream(ms);
        }

        return null;
    }
}
