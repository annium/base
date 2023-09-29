using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Annium.Core.Runtime.Loader.Internal;

internal class AssemblyLoaderBuilder : IAssemblyLoaderBuilder
{
    private readonly IList<Func<AssemblyName, string?>> _pathResolvers = new List<Func<AssemblyName, string?>>();
    private readonly IList<Func<AssemblyName, Task<byte[]>?>> _byteArrayResolvers = new List<Func<AssemblyName, Task<byte[]>?>>();

    public IAssemblyLoaderBuilder AddResolver(Func<AssemblyName, string?> pathResolver)
    {
        _pathResolvers.Add(pathResolver);

        return this;
    }

    public IAssemblyLoaderBuilder AddResolver(Func<AssemblyName, Task<byte[]>?> byteArrayResolver)
    {
        _byteArrayResolvers.Add(byteArrayResolver);

        return this;
    }

    public IAssemblyLoader Build()
    {
        return new AssemblyLoader(_pathResolvers.ToArray(), _byteArrayResolvers.ToArray());
    }
}