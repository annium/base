using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Annium.Core.DependencyInjection;
using Annium.Testing.Elements;

namespace Annium.Testing;

public static class AssemblyServicesCollector
{
    public static IServiceContainer Collect(Assembly assembly, IEnumerable<Test> tests)
    {
        var container = new ServiceContainer();

        // fixtures
        foreach (var type in assembly.GetTypes().Where(t => t.GetTypeInfo().GetCustomAttribute<FixtureAttribute>() != null))
            container.Add(type).AsSelf().Transient();

        // test classes
        foreach (var type in tests.Select(t => t.Method.DeclaringType!).Distinct())
            container.Add(type).AsSelf().Transient();

        return container;
    }
}