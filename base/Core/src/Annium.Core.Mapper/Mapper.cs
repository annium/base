using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
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
    /// so the underlying DI container's lifetime matches the cached singleton it produced. The value
    /// is wrapped in <see cref="Lazy{T}"/> with <see cref="LazyThreadSafetyMode.ExecutionAndPublication"/>
    /// semantics so <see cref="BuildEntry"/> runs exactly once per assembly — <c>ConcurrentDictionary.GetOrAdd</c>'s
    /// factory itself may execute multiple times under contention and would otherwise leak the losing
    /// races' <see cref="IServiceProvider"/> instances past <see cref="Clear"/>.
    /// </summary>
    private static readonly ConcurrentDictionary<Assembly, Lazy<(IMapper Mapper, IServiceProvider Provider)>> _entries =
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
                key => new Lazy<(IMapper Mapper, IServiceProvider Provider)>(
                    () => BuildEntry(key),
                    LazyThreadSafetyMode.ExecutionAndPublication
                )
            )
            .Value.Mapper;

    /// <summary>
    /// Builds a self-contained DI container hosting an <see cref="IMapper"/> for the given assembly.
    /// Returns the resolved mapper and the owning provider so the cache can dispose the provider
    /// in <see cref="Clear"/>. The container is intentionally NOT a <c>ServicePackBase</c> graph —
    /// there is no application container to register into, no Configure/Register/Setup phasing,
    /// and routing through the pack builder would force a synchronous <c>BuildAsync().GetAwaiter().GetResult()</c>
    /// at every <see cref="GetFor"/> call.
    /// </summary>
    /// <param name="assembly">Assembly the mapper resolves profiles from.</param>
    /// <returns>The mapper plus the owning service provider.</returns>
    private static (IMapper Mapper, IServiceProvider Provider) BuildEntry(Assembly assembly)
    {
        var container = new ServiceContainer();
        container.AddRuntime(assembly);
        container.AddMapper(false);
        // bind VoidLogger to ILogger so MapBuilder / TypeResolver / etc. can satisfy their ILogger ctor dep
        container.Add(VoidLogger.Instance).As<ILogger>().Singleton();

        var provider = container.BuildServiceProvider();

        return (provider.Resolve<IMapper>(), provider);
    }

    /// <summary>
    /// Disposes and removes all cached mapper providers. Intended for shutdown / test teardown;
    /// after Clear, any subsequent <see cref="GetFor"/> call builds a fresh container.
    /// </summary>
    public static void Clear()
    {
        foreach (var key in _entries.Keys)
        {
            // only dispose entries whose factory actually ran (IsValueCreated) — touching .Value here
            // would force-build a provider purely to dispose it
            if (
                _entries.TryRemove(key, out var entry)
                && entry.IsValueCreated
                && entry.Value.Provider is IDisposable disposable
            )
                disposable.Dispose();
        }
    }
}
