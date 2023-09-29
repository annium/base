using System;
using System.IO;
using System.Runtime.Loader;
using Annium.Core.Runtime.Loader.Internal;

namespace Annium.Core.Runtime.Loader;

public static class AssemblyLoaderBuilderExtensions
{
    public static IAssemblyLoaderBuilder UseFileSystemLoader(this IAssemblyLoaderBuilder builder, string directory) =>
        builder.AddResolver(assemblyName =>
        {
            var name = assemblyName.Name ?? throw new InvalidOperationException($"Can't resolve path for assembly {assemblyName}");
            var path = Helper.ToDllPath(directory, name);

            if (!File.Exists(path))
                return null;

            var resolver = new AssemblyDependencyResolver(Helper.ToDllPath(directory, name));

            return resolver.ResolveAssemblyToPath(assemblyName);
        });
}