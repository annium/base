using Annium.Core.DependencyInjection.Builders;
using Annium.Core.DependencyInjection.Container;
using Annium.Core.Runtime;
using Annium.Extensions.Validation.Internal;

namespace Annium.Extensions.Validation;

/// <summary>
/// Extension methods for configuring validation services in the dependency injection container
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Registers validation services in the dependency injection container
    /// </summary>
    /// <param name="container">The service container to configure</param>
    /// <returns>The service container for method chaining</returns>
    public static IServiceContainer AddValidation(this IServiceContainer container)
    {
        container.AddAll().AssignableTo(typeof(Validator<>)).Where(x => !x.IsGenericType).AsInterfaces().Scoped();

        container.Add(typeof(ValidationExecutor<>)).As(typeof(IValidator<>)).Scoped();

        return container;
    }
}
