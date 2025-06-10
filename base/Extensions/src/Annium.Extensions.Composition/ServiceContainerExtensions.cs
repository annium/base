using Annium.Core.DependencyInjection.Builders;
using Annium.Core.DependencyInjection.Container;
using Annium.Core.Runtime;
using Annium.Extensions.Composition.Internal;

namespace Annium.Extensions.Composition;

/// <summary>
/// Extension methods for configuring composition services in the dependency injection container
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Adds composition services to the service container, enabling value composition with configurable field rules
    /// </summary>
    /// <param name="container">The service container to configure</param>
    /// <returns>The service container for method chaining</returns>
    public static IServiceContainer AddComposition(this IServiceContainer container)
    {
        container.AddAll().AssignableTo(typeof(Composer<>)).Where(x => !x.IsGenericType).AsInterfaces().Scoped();

        container.Add(typeof(CompositionExecutor<>)).As(typeof(IComposer<>)).Scoped();

        return container;
    }
}
