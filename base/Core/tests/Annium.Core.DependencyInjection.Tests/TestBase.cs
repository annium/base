using System;
using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Extensions;

namespace Annium.Core.DependencyInjection.Tests;

/// <summary>
/// Base class for dependency injection tests providing common testing functionality
/// </summary>
public class TestBase
{
    /// <summary>
    /// The service container used for testing
    /// </summary>
    protected readonly ServiceContainer Container = new();

    /// <summary>
    /// The service provider built from the container
    /// </summary>
    private IServiceProvider _provider = default!;

    /// <summary>
    /// Builds the service provider from the container
    /// </summary>
    protected void Build()
    {
        _provider = Container.BuildServiceProvider();
    }

    /// <summary>
    /// Resolves a service of the specified type from the service provider
    /// </summary>
    /// <typeparam name="T">The type of service to resolve</typeparam>
    /// <returns>The resolved service instance</returns>
    protected T Get<T>()
        where T : notnull
    {
        return _provider.Resolve<T>();
    }

    /// <summary>
    /// Resolves a keyed service of the specified type from the service provider
    /// </summary>
    /// <typeparam name="T">The type of service to resolve</typeparam>
    /// <param name="key">The key to identify the service</param>
    /// <returns>The resolved service instance</returns>
    protected T GetKeyed<T>(object key)
        where T : notnull
    {
        return _provider.ResolveKeyed<T>(key);
    }
}
