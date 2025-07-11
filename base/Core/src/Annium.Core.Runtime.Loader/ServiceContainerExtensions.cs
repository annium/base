using Annium.Core.DependencyInjection;
using Annium.Core.Runtime.Loader.Internal;

namespace Annium.Core.Runtime.Loader;

/// <summary>
/// Extension methods for IServiceContainer to register assembly loader services
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Registers assembly loader services in the dependency injection container
    /// </summary>
    /// <param name="container">The service container to configure</param>
    /// <returns>The configured service container</returns>
    public static IServiceContainer AddAssemblyLoader(this IServiceContainer container)
    {
        container.Add<IAssemblyLoaderBuilder, AssemblyLoaderBuilder>().Transient();

        return container;
    }
}
