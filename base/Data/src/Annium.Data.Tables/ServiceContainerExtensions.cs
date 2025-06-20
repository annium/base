using Annium.Core.DependencyInjection;
using Annium.Data.Tables.Internal;

namespace Annium.Data.Tables;

/// <summary>
/// Extension methods for configuring table services in the dependency injection container.
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Adds table services to the dependency injection container.
    /// </summary>
    /// <param name="container">The service container to configure.</param>
    /// <returns>The service container for method chaining.</returns>
    public static IServiceContainer AddTables(this IServiceContainer container)
    {
        container.Add<ITableFactory, TableFactory>().Singleton();

        return container;
    }
}
