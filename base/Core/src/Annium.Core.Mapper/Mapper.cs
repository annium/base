using System.Collections.Concurrent;
using System.Reflection;
using Annium.Core.DependencyInjection;
using Annium.Logging;

namespace Annium.Core.Mapper;

/// <summary>
/// Static factory for creating mapper instances per assembly
/// </summary>
public static class Mapper
{
    /// <summary>
    /// Cache of mappers per assembly
    /// </summary>
    private static readonly ConcurrentDictionary<Assembly, IMapper> _mappers = new();

    /// <summary>
    /// Gets or creates a mapper instance for the specified assembly
    /// </summary>
    /// <param name="assembly">The assembly to get a mapper for</param>
    /// <returns>The mapper instance for the assembly</returns>
    public static IMapper GetFor(Assembly assembly) =>
        _mappers.GetOrAdd(
            assembly,
            x =>
            {
                var container = new ServiceContainer();
                container.AddRuntime(x);
                container.AddMapper(false);
                container.Add(VoidLogger.Instance).AsSelf().Singleton();

                var provider = container.BuildServiceProvider();

                return provider.Resolve<IMapper>();
            }
        );
}
