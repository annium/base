using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Annium.Core.Runtime.Loader.Internal;

/// <summary>
/// Internal implementation of assembly loader builder
/// </summary>
internal class AssemblyLoaderBuilder : IAssemblyLoaderBuilder
{
    /// <summary>
    /// Collection of path-based assembly resolvers
    /// </summary>
    private readonly List<Func<AssemblyName, string?>> _pathResolvers = new();

    /// <summary>
    /// Collection of byte array-based assembly resolvers
    /// </summary>
    private readonly List<Func<AssemblyName, byte[]?>> _byteArrayResolvers = new();

    /// <summary>
    /// Adds a path-based resolver for assembly loading
    /// </summary>
    /// <param name="pathResolver">Function that resolves assembly name to file path</param>
    /// <returns>The builder instance for method chaining</returns>
    public IAssemblyLoaderBuilder AddResolver(Func<AssemblyName, string?> pathResolver)
    {
        _pathResolvers.Add(pathResolver);

        return this;
    }

    /// <summary>
    /// Adds a byte array-based resolver for assembly loading
    /// </summary>
    /// <param name="byteArrayResolver">Function that resolves assembly name to byte array</param>
    /// <returns>The builder instance for method chaining</returns>
    public IAssemblyLoaderBuilder AddResolver(Func<AssemblyName, byte[]?> byteArrayResolver)
    {
        _byteArrayResolvers.Add(byteArrayResolver);

        return this;
    }

    /// <summary>
    /// Builds the configured assembly loader
    /// </summary>
    /// <returns>The configured assembly loader instance</returns>
    public IAssemblyLoader Build()
    {
        return new AssemblyLoader(_pathResolvers.ToArray(), _byteArrayResolvers.ToArray());
    }
}
