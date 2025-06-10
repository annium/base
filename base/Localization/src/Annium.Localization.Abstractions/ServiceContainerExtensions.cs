using System;
using Annium.Core.DependencyInjection.Container;

namespace Annium.Localization.Abstractions;

/// <summary>
/// Extension methods for configuring localization services
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Adds localization services to the service container
    /// </summary>
    /// <param name="container">The service container</param>
    /// <param name="configure">The localization configuration action</param>
    /// <returns>The service container for method chaining</returns>
    public static IServiceContainer AddLocalization(
        this IServiceContainer container,
        Action<LocalizationOptions> configure
    )
    {
        var options = new LocalizationOptions();
        configure(options);

        foreach (var service in options.LocaleStorageServices)
            container.Add(service);

        container.Add(options.CultureAccessor).AsSelf().Singleton();

        container.Add(typeof(Localizer<>)).As(typeof(ILocalizer<>)).Singleton();

        return container;
    }
}
