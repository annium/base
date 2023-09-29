using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;

namespace Annium.Core.Runtime.Loader.Internal;

internal class ResolvingLoadContext : AssemblyLoadContext
{
    private readonly IReadOnlyCollection<Func<AssemblyName, string?>> _pathResolvers;
    private readonly IReadOnlyCollection<Func<AssemblyName, Task<byte[]>?>> _byteArrayResolvers;

    public ResolvingLoadContext(
        IReadOnlyCollection<Func<AssemblyName, string?>> pathResolvers,
        IReadOnlyCollection<Func<AssemblyName, Task<byte[]>?>> byteArrayResolvers
    ) : base(true)
    {
        _pathResolvers = pathResolvers;
        _byteArrayResolvers = byteArrayResolvers;
    }

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
            using (var ms = new MemoryStream(byteArray))
                return LoadFromStream(ms);
        }

        return null;
    }
}