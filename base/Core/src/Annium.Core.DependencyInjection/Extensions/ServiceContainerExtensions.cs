// ReSharper disable once CheckNamespace
namespace Annium.Core.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static ISingleRegistrationBuilderBase Add<TService, TImplementation>(this IServiceContainer container)
        where TImplementation : TService
    {
        return container.Add(typeof(TImplementation)).As<TService>();
    }

    public static ISingleRegistrationBuilderBase Add<TImplementation>(this IServiceContainer container)
    {
        return container.Add(typeof(TImplementation));
    }

    public static IServiceContainer AddInjectables(this IServiceContainer container)
    {
        return container.Add(typeof(Injected<>)).AsSelf().Scoped();
    }
}
