using System;
using Annium.Core.DependencyInjection;

namespace Annium.Net.Mail;

/// <summary>
/// Extension methods for configuring email services in the service container
/// </summary>
public static class ServiceContainerExtensions
{
    /// <summary>
    /// Adds the email service implementation to the container as a singleton.
    /// The caller is responsible for registering <see cref="Configuration"/> separately.
    /// </summary>
    /// <param name="container">The service container</param>
    /// <returns>The service container for method chaining</returns>
    public static IServiceContainer AddEmailService(this IServiceContainer container)
    {
        container.Add<IEmailService, EmailService>().Singleton();

        return container;
    }

    /// <summary>
    /// Adds the email service implementation to the container as a singleton, configuring and
    /// registering its <see cref="Configuration"/> in the same call.
    /// </summary>
    /// <param name="container">The service container</param>
    /// <param name="configure">Delegate that configures the email service <see cref="Configuration"/></param>
    /// <returns>The service container for method chaining</returns>
    public static IServiceContainer AddEmailService(this IServiceContainer container, Action<Configuration> configure)
    {
        var configuration = new Configuration();
        configure(configuration);
        container.Add(configuration).AsSelf().Singleton();
        container.Add<IEmailService, EmailService>().Singleton();

        return container;
    }
}
