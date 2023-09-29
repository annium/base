using System.Collections.Concurrent;
using System.Reflection;
using Annium.Core.DependencyInjection;
using Annium.Logging;

namespace Annium.Core.Mapper;

public static class Mapper
{
    private static readonly ConcurrentDictionary<Assembly, IMapper> Mappers = new();

    public static IMapper GetFor(Assembly assembly) =>
        Mappers.GetOrAdd(assembly, x =>
        {
            var container = new ServiceContainer();
            container.AddRuntime(x);
            container.AddMapper(false);
            container.Add(VoidLogger.Instance).AsSelf().Singleton();

            var provider = container.BuildServiceProvider();

            return provider.Resolve<IMapper>();
        });
}