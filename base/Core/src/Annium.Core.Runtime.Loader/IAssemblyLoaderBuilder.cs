using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Annium.Core.Runtime.Loader;

public interface IAssemblyLoaderBuilder
{
    IAssemblyLoaderBuilder AddResolver(Func<AssemblyName, string?> pathResolver);
    IAssemblyLoaderBuilder AddResolver(Func<AssemblyName, Task<byte[]>?> byteArrayResolver);
    IAssemblyLoader Build();
}