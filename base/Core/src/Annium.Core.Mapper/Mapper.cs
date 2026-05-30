using System;
using System.Collections.Concurrent;
using System.Reflection;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Annium.Logging;

namespace Annium.Core.Mapper;

/// <summary>
/// Static factory for creating mapper instances per assembly. The factory caches both the
/// resolved <see cref="IMapper"/> and the owning <see cref="IServiceProvider"/> so the provider
/// is reachable for the lifetime of the cached mapper (and disposable via <see cref="Clear"/>).
/// </summary>
public static class Mapper
{
    /// <summary>
    /// Cache of (mapper, provider) tuples per assembly. The provider is held alongside the mapper
    /// so the underlying DI container's lifetime matches the cached singleton it produced.
    /// </summary>
    private static readonly ConcurrentDictionary<Assembly, (IMapper Mapper, IServiceProvider Provider)> _entries =
        new();

    /// <summary>
    /// Gets or creates a mapper instance for the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly to get a mapper for.</param>
    /// <returns>The mapper instance for the assembly.</returns>
    public static IMapper GetFor(Assembly assembly) =>
        _entries
            .GetOrAdd(
                assembly,
                x =>
                {
                    var container = new ServiceContainer();
                    container.AddRuntime(x);
                    container.AddMapper(false);
                    // bind VoidLogger to ILogger so MapBuilder / TypeResolver / etc. can satisfy their ILogger ctor dep
                    container.Add(VoidLogger.Instance).As<ILogger>().Singleton();

                    var provider = container.BuildServiceProvider();

                    return (provider.Resolve<IMapper>(), (IServiceProvider)provider);
                }
            )
            .Mapper;

    /// <summary>
    /// Disposes and removes all cached mapper providers. Intended for shutdown / test teardown;
    /// after Clear, any subsequent <see cref="GetFor"/> call builds a fresh container.
    /// </summary>
    public static void Clear()
    {
        foreach (var key in _entries.Keys)
        {
            if (_entries.TryRemove(key, out var entry) && entry.Provider is IDisposable disposable)
                disposable.Dispose();
        }
    }
}
