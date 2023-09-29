using Annium.Extensions.Validation;
using Annium.Extensions.Validation.Internal;

// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IServiceContainer AddValidation(this IServiceContainer container)
    {
        container.AddAll()
            .AssignableTo(typeof(Validator<>))
            .Where(x => !x.IsGenericType)
            .AsInterfaces()
            .Scoped();

        container.Add(typeof(ValidationExecutor<>)).As(typeof(IValidator<>)).Scoped();

        return container;
    }
}