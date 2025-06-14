using System.Reflection;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Annium.Localization.Abstractions;
using Annium.Localization.InMemory;

namespace Annium.Extensions.Validation.Tests;

/// <summary>
/// Base class for validation tests providing common setup and utility methods.
/// Configures the dependency injection container with validation and localization services.
/// </summary>
public class TestBase
{
    /// <summary>
    /// Creates and configures a validator instance for the specified type.
    /// Sets up a complete service container with runtime assembly scanning,
    /// validation services, and in-memory localization storage.
    /// </summary>
    /// <typeparam name="T">The type to create a validator for</typeparam>
    /// <returns>A configured validator instance for type T</returns>
    protected IValidator<T> GetValidator<T>() =>
        new ServiceContainer()
            .AddRuntime(Assembly.GetCallingAssembly())
            .AddValidation()
            .AddLocalization(opts => LocalizationOptionsExtensions.UseInMemoryStorage(opts))
            .BuildServiceProvider()
            .Resolve<IValidator<T>>();
}
