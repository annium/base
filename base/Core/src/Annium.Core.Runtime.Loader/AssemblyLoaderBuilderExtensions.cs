using System;
using System.IO;
using System.Runtime.Loader;
using Annium.Core.Runtime.Loader.Internal;

namespace Annium.Core.Runtime.Loader;

/// <summary>
/// Extension methods for IAssemblyLoaderBuilder to configure file system-based assembly loading
/// </summary>
public static class AssemblyLoaderBuilderExtensions
{
    /// <summary>
    /// Configures the assembly loader to resolve assemblies from a file system directory
    /// </summary>
    /// <param name="builder">The assembly loader builder to configure</param>
    /// <param name="directory">The directory path to search for assembly DLL files</param>
    /// <returns>The configured assembly loader builder</returns>
    public static IAssemblyLoaderBuilder UseFileSystemLoader(this IAssemblyLoaderBuilder builder, string directory) =>
        builder.AddResolver(assemblyName =>
        {
            var name =
                assemblyName.Name
                ?? throw new InvalidOperationException($"Can't resolve path for assembly {assemblyName}");
            var path = Helper.ToDllPath(directory, name);

            if (!File.Exists(path))
                return null;

            var resolver = new AssemblyDependencyResolver(Helper.ToDllPath(directory, name));

            return resolver.ResolveAssemblyToPath(assemblyName);
        });
}
