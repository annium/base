using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Annium.Core.Runtime.Loader;

/// <summary>
/// Builder interface for configuring and creating assembly loaders
/// </summary>
public interface IAssemblyLoaderBuilder
{
    /// <summary>
    /// Adds a path-based resolver for assembly loading
    /// </summary>
    /// <param name="pathResolver">Function that resolves assembly name to file path</param>
    /// <returns>The builder instance for method chaining</returns>
    IAssemblyLoaderBuilder AddResolver(Func<AssemblyName, string?> pathResolver);

    /// <summary>
    /// Adds a byte array-based resolver for assembly loading
    /// </summary>
    /// <param name="byteArrayResolver">Function that resolves assembly name to byte array</param>
    /// <returns>The builder instance for method chaining</returns>
    IAssemblyLoaderBuilder AddResolver(Func<AssemblyName, Task<byte[]>?> byteArrayResolver);

    /// <summary>
    /// Builds the configured assembly loader
    /// </summary>
    /// <returns>The configured assembly loader instance</returns>
    IAssemblyLoader Build();
}
